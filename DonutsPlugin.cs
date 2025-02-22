using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Donuts.Models;
using Donuts.PluginGUI;
using Donuts.Spawning.Utils;
using Donuts.Tools;
using Donuts.Utils;
using EFT.UI;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityToolkit.Utils;

namespace Donuts;

[BepInPlugin("com.dvize.Donuts", "Donuts", "2.0.0")]
[BepInDependency("com.SPT.core", "3.10.0")]
[BepInDependency("com.dvize.DonutsDependencyChecker")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
public class DonutsPlugin : BaseUnityPlugin
{
	private const KeyCode ESCAPE_KEY = KeyCode.Escape;
	
	internal static PluginGUIComponent pluginGUIComponent;
	internal static ConfigEntry<KeyboardShortcut> toggleGUIKey;
	
	private static readonly List<Folder> _emptyScenarioList = [];
	
	private bool _isWritingToFile;
	
	public new static ManualLogSource Logger { get; private set; }
	internal static ModulePatchManager ModulePatchManager { get; private set; }
	internal static string DirectoryPath { get; private set; }
	internal static Assembly CurrentAssembly { get; private set; }
	internal static bool FikaEnabled { get; private set; }
	
	private void Awake()
	{
		Logger = base.Logger;
		CurrentAssembly = Assembly.GetExecutingAssembly();
		string assemblyPath = CurrentAssembly.Location;
		DirectoryPath = Path.GetDirectoryName(assemblyPath);
		
		FikaEnabled = Chainloader.PluginInfos.Keys.Contains("com.fika.core");
		
		DonutsConfiguration.ImportConfig(DirectoryPath);
		
		toggleGUIKey = Config.Bind("配置设置", "启用/禁用配置界面的按键",
			new KeyboardShortcut(KeyCode.F9), "启用/禁用 Donuts 配置菜单的按键");
		
		ModulePatchManager = new ModulePatchManager(CurrentAssembly);
		ModulePatchManager.EnableAllPatches();
		
		ConsoleScreen.Processor.RegisterCommandGroup<SpawnCommands>();
	}
	
	// ReSharper disable once Unity.IncorrectMethodSignature
	[UsedImplicitly]
	private async UniTaskVoid Start()
	{
		await SetupScenariosUI();
		pluginGUIComponent = gameObject.AddComponent<PluginGUIComponent>();
		DonutsConfiguration.ExportConfig();
	}
	
	private void Update()
	{
		// If setting a keybind, do not trigger functionality
		if (ImGUIToolkit.IsSettingKeybind()) return;
		
		ShowGuiInputCheck();
		
		if (IsKeyPressed(DefaultPluginVars.CreateSpawnMarkerKey.Value))
		{
			EditorFunctions.CreateSpawnMarker();
		}
		if (IsKeyPressed(DefaultPluginVars.WriteToFileKey.Value) && !_isWritingToFile)
		{
			_isWritingToFile = true;
			EditorFunctions.WriteToJsonFile(DirectoryPath)
				.ContinueWith(() => _isWritingToFile = false)
				.Forget();
		}
		if (IsKeyPressed(DefaultPluginVars.DeleteSpawnMarkerKey.Value))
		{
			EditorFunctions.DeleteSpawnMarker();
		}
	}
	
	private static void ShowGuiInputCheck()
	{
		if (IsKeyPressed(toggleGUIKey.Value) || IsKeyPressed(ESCAPE_KEY))
		{
			if (!IsKeyPressed(ESCAPE_KEY))
			{
				DefaultPluginVars.ShowGUI = !DefaultPluginVars.ShowGUI;
			}
			// Check if the config window is open
			else if (DefaultPluginVars.ShowGUI)
			{
				DefaultPluginVars.ShowGUI = false;
			}
		}
	}
	
	private static async UniTask SetupScenariosUI()
	{
		await LoadDonutsScenarios();
		
		// Dynamically initialize the scenario settings
		DefaultPluginVars.pmcScenarioSelection = new Setting<string>("PMC 战局生成预设选择",
            "选择一个预设,用于以PMC身份生成时使用.",
			DefaultPluginVars.PmcScenarioSelectionValue ?? "live-like",
			"live-like",
			options: DefaultPluginVars.pmcScenarioCombinedArray);
		
		DefaultPluginVars.scavScenarioSelection = new Setting<string>("SCAV 战局生成预设选择",
            "选择一个预设,用于以SCAV身份生成时使用",
			DefaultPluginVars.ScavScenarioSelectionValue ?? "live-like",
			"live-like",
			options: DefaultPluginVars.scavScenarioCombinedArray);
	}
	
	private static async UniTask LoadDonutsScenarios()
	{
		// TODO: Write a null check in case the files are missing and generate new defaults
		
		string scenarioConfigPath = Path.Combine(DirectoryPath, "ScenarioConfig.json");
		DefaultPluginVars.PmcScenarios = await LoadFoldersAsync(scenarioConfigPath);
		
		string randomScenarioConfigPath = Path.Combine(DirectoryPath, "RandomScenarioConfig.json");
		DefaultPluginVars.PmcRandomScenarios = await LoadFoldersAsync(randomScenarioConfigPath);
		
		DefaultPluginVars.ScavScenarios = DefaultPluginVars.PmcScenarios;
		DefaultPluginVars.ScavRandomScenarios = DefaultPluginVars.PmcRandomScenarios;
		
		PopulateScenarioValues();
		
		if (DefaultPluginVars.debugLogging.Value)
		{
			Logger.LogWarning($"已加载的 PMC 场景: {string.Join(", ", DefaultPluginVars.pmcScenarioCombinedArray)}");
			Logger.LogWarning($"已加载的 Scav 场景: {string.Join(", ", DefaultPluginVars.scavScenarioCombinedArray)}");
		}
	}
	
	private static async UniTask<List<Folder>> LoadFoldersAsync([NotNull] string filePath)
	{
		if (!File.Exists(filePath))
		{
			Logger.LogError($"文件未找到: {filePath}");
			return _emptyScenarioList;
		}
		
		string fileContent = await DonutsHelper.ReadAllTextAsync(filePath);
		var folders = JsonConvert.DeserializeObject<List<Folder>>(fileContent);
		
		if (folders == null || folders.Count == 0)
		{
			Logger.LogError($"在以下位置的场景配置文件中未找到 Donuts 文件夹: {filePath}");
			return _emptyScenarioList;
		}
		
		Logger.LogWarning($"已加载 {folders.Count.ToString()} 个 Donuts 场景文件夹");
		return folders;
	}
	
	private static void PopulateScenarioValues()
	{
		DefaultPluginVars.pmcScenarioCombinedArray = GenerateScenarioValues(DefaultPluginVars.PmcScenarios, DefaultPluginVars.PmcRandomScenarios);
		Logger.LogWarning($"已加载 {DefaultPluginVars.pmcScenarioCombinedArray.Length.ToString()} 个 PMC 场景并完成生成");
		
		DefaultPluginVars.scavScenarioCombinedArray = GenerateScenarioValues(DefaultPluginVars.ScavScenarios, DefaultPluginVars.ScavRandomScenarios);
		Logger.LogWarning($"已加载 {DefaultPluginVars.scavScenarioCombinedArray.Length.ToString()} 个 SCAV 场景并完成生成");
	}
	
	private static string[] GenerateScenarioValues([NotNull] List<Folder> scenarios, [NotNull] List<Folder> randomScenarios)
	{
		var scenarioValues = new string[scenarios.Count + randomScenarios.Count];
		var pointer = 0;

		foreach (Folder scenario in scenarios)
		{
			scenarioValues[pointer] = scenario.Name;
			pointer++;
		}
		
		foreach (Folder scenario in randomScenarios)
		{
			scenarioValues[pointer] = scenario.RandomScenarioConfig;
			pointer++;
		}
		
		return scenarioValues;
	}
	
	private static bool IsKeyPressed(KeyboardShortcut key)
	{
		bool isMainKeyDown = UnityInput.Current.GetKeyDown(key.MainKey);
		var allModifierKeysDown = true;
		
		foreach (KeyCode keyCode in key.Modifiers)
		{
			if (!UnityInput.Current.GetKey(keyCode))
			{
				allModifierKeysDown = false;
				break;
			}
		}
		
		return isMainKeyDown && allModifierKeysDown;
	}
	
	private static bool IsKeyPressed(KeyCode key) => UnityInput.Current.GetKeyDown(key);
}