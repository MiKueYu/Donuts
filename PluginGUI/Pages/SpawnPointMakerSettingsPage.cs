namespace Donuts.PluginGUI.Pages;

internal class SpawnPointMakerSettingsPage : TabContainerPage
{
    public override string Name => "生成点创建器";

    public SpawnPointMakerSettingsPage() : base(PluginGUIComponent.SubTabButtonStyle, PluginGUIComponent.SubTabButtonActiveStyle)
    {
        Tabs.Add(new KeybindsTabSettingsPage());
        Tabs.Add(new SpawnSetupTabSettingsPage());
    }
}