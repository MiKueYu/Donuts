﻿using EFT;
using HarmonyLib;
using JetBrains.Annotations;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Donuts.Patches;

[UsedImplicitly]
internal class BotMemoryAddEnemyPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod() => AccessTools.Method(typeof(BotMemoryClass), nameof(BotMemoryClass.AddEnemy));

	[PatchPrefix]
	private static bool PatchPrefix(IPlayer enemy)
	{
		if (enemy == null || (enemy.IsAI && enemy.AIData?.BotOwner?.GetPlayer == null))
		{
			return false;
		}
		return true;
	}
}