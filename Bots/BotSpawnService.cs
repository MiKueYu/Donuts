﻿using BepInEx.Logging;
using Comfort.Common;
using Cysharp.Threading.Tasks;
using Donuts.Bots.SpawnCheckProcessor;
using Donuts.Models;
using Donuts.Utils;
using EFT;
using JetBrains.Annotations;
using SPT.SinglePlayer.Utils.InRaid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Systems.Effects;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Donuts.Bots;

public interface IBotSpawnService
{
	void FrameUpdate(float deltaTime);
	UniTask SpawnStartingBots();
	List<BotWave> GetBotWavesToSpawn();
	UniTask<bool> TrySpawnBotWave(BotWave wave);
	bool TryDespawnFurthestBot();
	void RestartPlayerHitTimer();
}

public abstract class BotSpawnService : IBotSpawnService
{
	private BotSpawner _eftBotSpawner;
	private CancellationToken _onDestroyToken;
	
	private SpawnCheckProcessorBase _spawnCheckProcessor;
	
	private GameWorld _gameWorld;
	private string _mapLocation;
	private BotsController _botsController;
	private IBotCreator _botCreator;
	
	private float _despawnCooldownTime;
	
	// Combat state
	private float _timeSinceLastHit;
	
	// Spawn caps
	private int _currentRespawnCount;

	protected BotConfigService ConfigService { get; private set; }
	protected IBotDataService DataService { get; private set; }
	protected ManualLogSource Logger { get; private set; }
	protected MapBotWaves MapBotWaves { get; private set; }

	public static TBotSpawnService Create<TBotSpawnService>(
		[NotNull] BotConfigService configService,
		[NotNull] IBotDataService dataService,
		[NotNull] BotSpawner eftBotSpawner,
		[NotNull] ManualLogSource logger,
		CancellationToken cancellationToken = default)
	where TBotSpawnService : BotSpawnService, new()
	{
		var service = new TBotSpawnService();
		service.Initialize(configService, dataService, eftBotSpawner, logger, cancellationToken);
		return service;
	}

	public void FrameUpdate(float deltaTime)
	{
		_timeSinceLastHit += deltaTime;
		UpdateBotWaveTimers(deltaTime);
	}

	public void RestartPlayerHitTimer()
	{
		_timeSinceLastHit = 0;
	}

	protected virtual void Initialize(
		[NotNull] BotConfigService configService,
		[NotNull] IBotDataService dataService,
		[NotNull] BotSpawner eftBotSpawner,
		[NotNull] ManualLogSource logger,
		CancellationToken cancellationToken)
	{
		ConfigService = configService;
		DataService = dataService;
		_eftBotSpawner = eftBotSpawner;
		Logger = logger;
		_onDestroyToken = cancellationToken;
		
		_gameWorld = Singleton<GameWorld>.Instance;
		_mapLocation = ConfigService.GetMapLocation();
		_botsController = Singleton<IBotGame>.Instance.BotsController;
		_botCreator = (IBotCreator)ReflectionHelper.BotSpawner_botCreator_Field.GetValue(_eftBotSpawner);
		
		var isBotSpawningEnabled = (bool)ReflectionHelper.BotsController_botEnabled_Field
			.GetValue(Singleton<IBotGame>.Instance.BotsController);
		if (!isBotSpawningEnabled || !ConfigService.CheckForAnyScenarioPatterns())
		{
			return;
		}

		MapBotWaves = ConfigService.GetBotWavesConfig()?.Maps[_mapLocation];
		if (MapBotWaves == null)
		{
			Logger.NotifyLogError("Donuts: Failed to load bot waves. Donuts will not function properly.");
			return;
		}
		
		_spawnCheckProcessor = new PlayerVicinitySpawnCheckProcessor();
		_spawnCheckProcessor.SetNext(new BotVicinitySpawnCheckProcessor())
			.SetNext(new PlayerLineOfSightSpawnCheckProcessor())
			.SetNext(new WallSpawnCheckProcessor())
			.SetNext(new GroundSpawnCheckProcessor());
	}

	protected abstract bool IsHardStopEnabled();
	protected abstract int GetHardStopTime();

	// TODO: why is this method failing?
	// This is an old comment, method has been changed so it needs testing
	private bool HasReachedHardStopTime()
	{
		if (!IsHardStopEnabled())
		{
			return false;
		}
		
		if (DefaultPluginVars.useTimeBasedHardStop.Value)
		{
			float raidTimeLeftTime = RaidTimeUtil.GetRemainingRaidSeconds(); // Time left
			int hardStopTime = GetHardStopTime();
#if DEBUG
			Logger.LogDebug(string.Format("RaidTimeLeftTime: {0}, HardStopTime: {1}",
				raidTimeLeftTime.ToString(CultureInfo.InvariantCulture), hardStopTime.ToString()));
#endif
			return raidTimeLeftTime <= hardStopTime;
		}

		float raidTimeLeftPercent = RaidTimeUtil.GetRaidTimeRemainingFraction() * 100f; // Percent left
		int hardStopPercent = GetHardStopTime();
#if DEBUG
		Logger.LogDebug(string.Format("RaidTimeLeftPercent: {0}, HardStopPercent: {1}",
			raidTimeLeftPercent.ToString(CultureInfo.InvariantCulture), hardStopPercent.ToString()));
#endif
		return raidTimeLeftPercent <= hardStopPercent;
	}

	protected abstract List<BotWave> GetBotWavesList();

	private void UpdateBotWaveTimers(float deltaTime)
	{
		float cooldownDuration = DefaultPluginVars.coolDownTimer.Value;
		foreach (BotWave wave in GetBotWavesList())
		{
			wave.UpdateTimer(deltaTime, cooldownDuration);
		}
	}

	[CanBeNull]
	private AICorePoint GetClosestCorePoint(Vector3 position) =>
		_botsController.CoversData.GetClosest(position)?.CorePointInGame;

	private void ActivateBotAtPosition(
		[NotNull] BotCreationDataClass botData,
		Vector3 spawnPosition)
	{
		BotZone closestBotZone = _eftBotSpawner.GetClosestZone(spawnPosition, out _);
		AICorePoint closestCorePoint = GetClosestCorePoint(spawnPosition);
		botData.AddPosition(spawnPosition, closestCorePoint!.Id);
		
		var createBotCallbackWrapper = new CreateBotCallbackWrapper(botData);
		var getGroupWrapper = new GetBotsGroupWrapper(_eftBotSpawner);
		var groupAction = new Func<BotOwner, BotZone, BotsGroup>(getGroupWrapper.GetGroupAndSetEnemies);
		var callback = new Action<BotOwner>(createBotCallbackWrapper.CreateBotCallback);

		_botCreator.ActivateBot(botData, closestBotZone, false, groupAction, callback, _onDestroyToken);
		DataService.ClearBotCache(botData);
		//DataService.ReplenishBotDataTimer.Restart();
	}

	public async UniTask SpawnStartingBots()
	{
		foreach (BotSpawnInfo botSpawnInfo in DataService.BotSpawnInfos)
		{
			if (_onDestroyToken.IsCancellationRequested) return;
			
			await SpawnBot(botSpawnInfo.GroupSize, botSpawnInfo.Zone, botSpawnInfo.Coordinates);
		}
	}

	public List<BotWave> GetBotWavesToSpawn()
	{
		List<BotWave> botWaves = GetBotWavesList();
		List<BotWave> wavesToSpawn = new(botWaves.Count);
		foreach (BotWave wave in botWaves)
		{
			if (wave.ShouldSpawn())
			{
				wavesToSpawn.Add(wave);
			}
		}
		return wavesToSpawn;
	}

	public async UniTask<bool> TrySpawnBotWave(BotWave wave)
	{
		var anySpawned = false;
		if (IsHumanPlayerInCombat())
		{
#if DEBUG
			Logger.LogDebug("In combat state cooldown, breaking the loop.");
#endif
			ResetGroupTimers(wave.GroupNum);
			return false;
		}

		if (CanSpawn(wave.SpawnChance) && await TryProcessBotWave(wave))
		{
			anySpawned = true;
		}
			
		ResetGroupTimers(wave.GroupNum);
		return anySpawned;
	}
	
	protected abstract bool IsCorrectSpawnType(WildSpawnType role);
	
	[CanBeNull]
	private Player FindFurthestBot()
	{
		var furthestSqrMagnitude = float.MinValue;
		Player furthestBot = null;
		List<Player> allAlivePlayers = _gameWorld.AllAlivePlayersList;
		List<Player> humanPlayers = ConfigService.GetHumanPlayerList();
		for (int i = allAlivePlayers.Count - 1; i >= 0; i--)
		{
			Player player = allAlivePlayers[i];
			if (!player.IsAI || player.AIData.BotOwner == null || IsCorrectSpawnType(player.Profile.Info.Settings.Role))
			{
				continue;
			}

			for (int j = humanPlayers.Count - 1; j >= 0; j--)
			{
				Player humanPlayer = humanPlayers[j];
				
				// Get distance of bot to player using squared distance
				float sqrMagnitude = (humanPlayer.Transform.position - player.Transform.position).sqrMagnitude;

				// Check if this is the furthest distance
				if (sqrMagnitude > furthestSqrMagnitude)
				{
					furthestSqrMagnitude = sqrMagnitude;
					furthestBot = player;
				}
			}
		}

#if DEBUG
		if (furthestBot == null)
		{
			Logger.LogDebug("Furthest bot is null. No bots found in the list.");
		}
		else
		{
			Logger.LogDebug(string.Format("Furthest bot found: {0} at distance {1}", furthestBot.Profile.Info.Nickname,
				Mathf.Sqrt(furthestSqrMagnitude).ToString(CultureInfo.InvariantCulture)));
		}
#endif

		return furthestBot;
	}
	
	// Only consider despawning if the number of active bots of the type exceeds the limit
	private bool CanDespawnBot() => GetAliveBotsCount() > DataService.MaxBotLimit;

	public bool TryDespawnFurthestBot()
	{
		if (!IsDespawnBotEnabled())
		{
			return false;
		}

		float currentTime = Time.time;
		float timeSinceLastDespawn = currentTime - _despawnCooldownTime;

		if (timeSinceLastDespawn < DefaultPluginVars.despawnInterval.Value || !CanDespawnBot())
		{
			return false;
		}

		Player furthestBot = FindFurthestBot();

		if (furthestBot != null)
		{
			return TryDespawnBot(furthestBot);
		}
#if DEBUG
		Logger.LogDebug($"No {DataService.SpawnType.ToString()} bot found to despawn.");
		return false;
#endif
	}

	protected abstract bool IsDespawnBotEnabled();

	private bool TryDespawnBot([NotNull] Player furthestBot)
	{
		if (furthestBot == null)
		{
			Logger.LogError("Attempted to despawn a null bot.");
			return false;
		}

		BotOwner botOwner = furthestBot.AIData.BotOwner;
		if (botOwner == null)
		{
			Logger.LogError("BotOwner is null for the furthest bot.");
			return false;
		}

#if DEBUG
		Logger.LogDebug($"Despawning bot: {furthestBot.Profile.Info.Nickname} ({furthestBot.name})");
#endif

		_gameWorld.RegisteredPlayers.Remove(botOwner);
		_gameWorld.AllAlivePlayersList.Remove(botOwner.GetPlayer);

		IBotGame botGame = Singleton<IBotGame>.Instance;
		Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(botOwner.GetPlayer);
		botOwner.Deactivate();
		botOwner.Dispose();
		botGame.BotsController.BotDied(botOwner);
		botGame.BotsController.DestroyInfo(botOwner.GetPlayer);
		//DestroyImmediate(botOwner.gameObject);
		Object.Destroy(botOwner.gameObject);
		//Destroy(botOwner);

		// Update the cooldown
		_despawnCooldownTime = Time.time;
		return true;
	}

	private bool IsHumanPlayerInCombat()
	{
		return _timeSinceLastHit < DefaultPluginVars.battleStateCoolDown.Value;
	}

	private bool CanSpawn(int spawnChance)
    {
    	int randomValue = Random.Range(0, 100);
    	bool canSpawn = randomValue < spawnChance;
#if DEBUG
    	Logger.LogDebug(string.Format("SpawnChance: {0}, RandomValue: {1}, CanSpawn: {2}", spawnChance.ToString(),
		    randomValue.ToString(), canSpawn.ToString()));
#endif
    	return canSpawn;
    }

	private async UniTask<bool> TryProcessBotWave(BotWave botWave)
	{
		Dictionary<string, List<Vector3>> zoneSpawnPoints = DataService.ZoneSpawnPoints;
		if (!zoneSpawnPoints.Any())
		{
#if DEBUG
			Logger.LogDebug("No zone spawn points found, cannot spawn any bot waves.");
#endif
			return false;
		}

		string randomZone = zoneSpawnPoints.Keys.PickRandomElement()!;
		List<Vector3> spawnPoints = zoneSpawnPoints[randomZone].ShuffleElements();

		AdjustSpawnChanceIfHotspot(botWave, randomZone);

		foreach (Vector3 spawnPoint in spawnPoints)
		{
			if (_onDestroyToken.IsCancellationRequested)
			{
				return false;
			}
			
#if DEBUG
			Logger.LogDebug($"Triggering spawn for botWave: {botWave} at {randomZone}, {spawnPoint.ToString()}");
#endif
			if (await TrySpawnBot(botWave, randomZone, spawnPoint, spawnPoints))
			{
				return true;
			}
		}
		return false;
	}

	private void ResetGroupTimers(int groupNum)
	{
		foreach (BotWave botWave in GetBotWavesList())
		{
			if (botWave.GroupNum == groupNum)
			{
				botWave.ResetTimer();
#if DEBUG
				Logger.LogDebug($"Resetting timer for GroupNum: {groupNum.ToString()}");
#endif
			}
		}
	}
	
	protected abstract bool IsHotspotBoostEnabled();

	/// <summary>
	/// Sets the bot wave's spawn chance to 100% if the zone is a hotspot.
	/// </summary>
	private void AdjustSpawnChanceIfHotspot(BotWave botWave, string zone)
	{
		if (!IsHotspotBoostEnabled()) return;
		
		bool isHotspotZone = zone.IndexOf("hotspot", StringComparison.OrdinalIgnoreCase) >= 0;
		if (!isHotspotZone) return;
		
#if DEBUG
		Logger.LogDebug($"{zone} is a hotspot; hotspot boost is enabled, setting spawn chance to 100");
#endif
		botWave.SpawnChance = 100;
	}


	private bool IsHumanPlayerWithinTriggerDistance(int triggerDistance, Vector3 position)
	{
		int triggerSqrMagnitude = triggerDistance * triggerDistance;

		List<Player> humanPlayerList = ConfigService.GetHumanPlayerList();
		for (int i = humanPlayerList.Count - 1; i >= 0; i--)
		{
			Player player = humanPlayerList[i];
			if (player == null || player.HealthController == null || player.HealthController.IsAlive == false) continue;

			float sqrMagnitude = (((IPlayer)player).Position - position).sqrMagnitude;
			if (sqrMagnitude <= triggerSqrMagnitude)
			{
				return true;
			}
		}
		return false;
	}
	
	protected abstract bool HasReachedHardCap();
	
	/// <summary>
	/// Checks certain spawn options, reset groups timers.
	/// </summary>
	private async UniTask<bool> TrySpawnBot(
		BotWave botWave,
		string zone,
		Vector3 primarySpawnPoint,
		List<Vector3> spawnPoints)
	{
		if (!IsHumanPlayerWithinTriggerDistance(botWave.TriggerDistance, primarySpawnPoint) ||
			(DefaultPluginVars.HardCapEnabled.Value && HasReachedHardCap()) ||
			HasReachedHardStopTime())
		{
			ResetGroupTimers(botWave.GroupNum); // Reset timer if the wave is hard capped
			return false;
		}
		
		int groupSize = DetermineBotGroupSize(botWave.MinGroupSize, botWave.MaxGroupSize);
		if (groupSize < 1)
		{
			return false;
		}

		if (await SpawnBot(groupSize, zone, spawnPoints))
		{
			botWave.TimesSpawned++;
		}
		
		if (_onDestroyToken.IsCancellationRequested)
		{
			return false;
		}
		
		ResetGroupTimers(botWave.GroupNum);

		if (botWave.TimesSpawned >= botWave.MaxTriggersBeforeCooldown)
		{
			botWave.TriggerCooldown();
		}
		return true;
	}

	private async UniTask<bool> SpawnBot(
		int groupSize,
		string zone,
		List<Vector3> spawnPoints,
		bool ignoreChecks = false)
	{
		bool isGroup = groupSize > 1;
#if DEBUG
		Logger.LogDebug($"Attempting to spawn {(isGroup ? "group" : "solo")} bot {groupSize.ToString()} in spawn zone {zone}");
#endif
		
		BotDifficulty botDifficulty = DataService.GetBotDifficulty();

		BotCreationDataClass cachedBotData = DataService.FindCachedBotData(botDifficulty, groupSize);
		if (cachedBotData == null)
		{
#if DEBUG
			Logger.LogWarning(string.Format(
				"No cached bots found for this spawn, generating on the fly for {0} bots - this may take some time.",
				groupSize.ToString()));
#endif
			var botInfo = new PrepBotInfo(botDifficulty, isGroup, groupSize);
			(bool success, cachedBotData) = await DataService.TryCreateBotData(botInfo);
			if (_onDestroyToken.IsCancellationRequested || !success)
			{
				return false;
			}
		}
#if DEBUG
		else Logger.LogWarning("Found grouped cached bots, spawning them.");
#endif

		var spawned = false;
		foreach (Vector3 position in spawnPoints)
		{
			Vector3 spawnPosition = await GetValidSpawnPosition(ignoreChecks, position);
			if (_onDestroyToken.IsCancellationRequested)
			{
				return false;
			}
			
			if (spawnPosition != Vector3.zero)
			{
				ActivateBotAtPosition(cachedBotData, spawnPosition);
				spawned = true;
			}
		}
		
#if DEBUG
		if (!spawned)
		{
			Logger.LogDebug("No valid spawn position found after retries - skipping this spawn");
		}
#endif
		return spawned;
	}

	private async UniTask<Vector3> GetValidSpawnPosition(bool ignoreChecks, Vector3 position)
	{
		int maxSpawnAttempts = DefaultPluginVars.maxSpawnTriesPerBot.Value;
		for (var i = 0; i < maxSpawnAttempts; i++)
		{
			if (_onDestroyToken.IsCancellationRequested)
			{
				return Vector3.zero;
			}

			if (NavMesh.SamplePosition(position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
			{
				if (!ignoreChecks)
				{
					var spawnCheckData = new SpawnCheckData(navHit.position, _mapLocation,
						_gameWorld.AllAlivePlayersList);
					_spawnCheckProcessor.Process(spawnCheckData);
					if (!spawnCheckData.Success)
					{
						return Vector3.zero;
					}
				}
					
				Vector3 spawnPosition = navHit.position;
#if DEBUG
				Logger.LogDebug($"Found spawn position at: {spawnPosition.ToString()}");
#endif
				return spawnPosition;
			}
				
			await UniTask.Yield(_onDestroyToken);
		}

		return Vector3.zero;
	}

	protected abstract int GetBotGroupSize(int minGroupSize, int maxGroupSize);

	private int DetermineBotGroupSize(int minGroupSize, int maxGroupSize)
	{
		int groupSize = GetBotGroupSize(minGroupSize, maxGroupSize);

		// Check if hard cap is enabled and adjust maxCount based on active bot counts and limits
		if (DefaultPluginVars.HardCapEnabled.Value)
		{
			groupSize = AdjustGroupSizeForHardCap(groupSize);
		}
		
		// Check respawn limits and adjust accordingly
		groupSize = AdjustMaxCountForRespawnLimits(groupSize);

		if (groupSize < 1)
		{
			return -1;
		}
		return groupSize;
	}

	protected abstract int GetAliveBotsCount();

	private int AdjustGroupSizeForHardCap(int groupSize)
	{
		int activeBots = GetAliveBotsCount();
		if (activeBots + groupSize > DataService.MaxBotLimit)
		{
			groupSize = DataService.MaxBotLimit - activeBots;
		}
		return groupSize;
	}

	protected abstract int GetMaxBotRespawns();

	private int AdjustMaxCountForRespawnLimits(int groupSize)
	{
		int maxBotRespawns = GetMaxBotRespawns();
		if (maxBotRespawns <= 0)
		{
			_currentRespawnCount += groupSize;
			return groupSize;
		}

		if (_currentRespawnCount >= maxBotRespawns)
		{
#if DEBUG
			Logger.LogDebug($"Max {DataService.SpawnType.ToString()} respawns reached, skipping this spawn");
#endif
			return -1;
		}

		if (_currentRespawnCount + groupSize >= maxBotRespawns)
		{
#if DEBUG
			Logger.LogDebug(
				string.Format("Max {0} respawn limit reached: {1}. Current {0} respawns this raid: {2}",
					DataService.SpawnType.ToString(), DefaultPluginVars.maxRespawnsPMC.Value.ToString(),
					(_currentRespawnCount + groupSize).ToString()));
#endif
			groupSize = maxBotRespawns - _currentRespawnCount;
			
		}
		_currentRespawnCount += groupSize;
		return groupSize;
	}
}