using Donuts.Models;
using System.Collections.Generic;
using UnityEngine;
using static Donuts.DefaultPluginVars;
using static Donuts.PluginGUI.ImGUIToolkit;

namespace Donuts.PluginGUI.Pages;

internal class SpawnSettingsPage : ISettingsPage
{
    public string Name => "生成设置";

    public void Draw()
    {
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        Accordion("全局与玩家最小距离设置", "点击展开/折叠", () =>
        {
            // Toggle for globalMinSpawnDistanceFromPlayerBool
            globalMinSpawnDistanceFromPlayerBool.Value = Toggle(globalMinSpawnDistanceFromPlayerBool.Name,
                globalMinSpawnDistanceFromPlayerBool.ToolTipText, globalMinSpawnDistanceFromPlayerBool.Value);

            // List of float settings
            var floatSettings = new List<Setting<float>>
            {
                globalMinSpawnDistanceFromPlayerFactory,
                globalMinSpawnDistanceFromPlayerCustoms,
                globalMinSpawnDistanceFromPlayerReserve,
                globalMinSpawnDistanceFromPlayerStreets,
                globalMinSpawnDistanceFromPlayerWoods,
                globalMinSpawnDistanceFromPlayerLaboratory,
                globalMinSpawnDistanceFromPlayerShoreline,
                globalMinSpawnDistanceFromPlayerGroundZero,
                globalMinSpawnDistanceFromPlayerInterchange,
                globalMinSpawnDistanceFromPlayerLighthouse
            };

            // Sort the settings by name in ascending order
            floatSettings.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            // Create sliders for the sorted settings
            foreach (Setting<float> setting in floatSettings)
            {
	            setting.Value = Slider(setting.Name, setting.ToolTipText, setting.Value, 0f, 1000f);
            }
        });

        Accordion(" 全局与其他机器人最小距离设置", "点击展开/折叠", () =>
        {
            // Toggle for globalMinSpawnDistanceFromOtherBotsBool
            globalMinSpawnDistanceFromOtherBotsBool.Value = Toggle(
                globalMinSpawnDistanceFromOtherBotsBool.Name,
                globalMinSpawnDistanceFromOtherBotsBool.ToolTipText,
                globalMinSpawnDistanceFromOtherBotsBool.Value
            );

            // List of float settings for other bots
            var otherBotsFloatSettings = new List<Setting<float>>
            {
                globalMinSpawnDistanceFromOtherBotsFactory,
                globalMinSpawnDistanceFromOtherBotsCustoms,
                globalMinSpawnDistanceFromOtherBotsReserve,
                globalMinSpawnDistanceFromOtherBotsStreets,
                globalMinSpawnDistanceFromOtherBotsWoods,
                globalMinSpawnDistanceFromOtherBotsLaboratory,
                globalMinSpawnDistanceFromOtherBotsShoreline,
                globalMinSpawnDistanceFromOtherBotsGroundZero,
                globalMinSpawnDistanceFromOtherBotsInterchange,
                globalMinSpawnDistanceFromOtherBotsLighthouse
            };

            // Sort the settings by name in ascending order
            otherBotsFloatSettings.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            // Create sliders for the sorted settings
            foreach (Setting<float> setting in otherBotsFloatSettings)
            {
	            setting.Value = Slider(setting.Name, setting.ToolTipText, setting.Value, 0f, 1000f);
            }
        });

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}