namespace Donuts.PluginGUI.Pages;

internal class MainSettingsPage : TabContainerPage
{
	public override string Name => "主要设置";

	public MainSettingsPage() : base(PluginGUIComponent.SubTabButtonStyle, PluginGUIComponent.SubTabButtonActiveStyle)
	{
		Tabs.Add(new MainSettingsGeneralPage());
		Tabs.Add(new MainSettingsSpawnFrequencyPage());
		Tabs.Add(new MainSettingsBotAttributesPage());
	}
}