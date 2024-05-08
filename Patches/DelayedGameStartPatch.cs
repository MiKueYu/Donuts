﻿using System;
using System.Collections;
using System.Reflection;
using Aki.Reflection.Patching;
using Donuts;
using UnityEngine;

namespace Donuts.Patches
{
    public class DelayedGameStartPatch : ModulePatch
    {
        private static object localGameObj = null;
        protected override MethodBase GetTargetMethod()
        {
            Type localGameType = Aki.Reflection.Utils.PatchConstants.LocalGameType;
            return localGameType.GetMethod("method_18", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void PatchPostfix(ref IEnumerator __result, object __instance, float startDelay)
        {
            localGameObj = __instance;
            __result = addIterationsToWaitForBotGenerators(__result);  //thanks danW
        }
        private static IEnumerator addIterationsToWaitForBotGenerators(IEnumerator originalTask)
        {
            // Now also wait for all bots to be fully initialized
            Logger.LogWarning("Donuts is waiting for bot preparation to complete...");
            while (!DonutsBotPrep.IsBotPreparationComplete)
            {
                yield return new WaitForSeconds(0.5f); // Check every 500ms
                Logger.LogInfo("Donuts is processing still.");
            }

            // Continue with the original task
            yield return originalTask;
            Logger.LogWarning("Donuts bot preparation is complete...");
        }

        
    }
}
