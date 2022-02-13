using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.HttpMonitor.Settings;

public class HideURLsConfigurable : IConfigurable
{
    public string GetValue(ConfigurableContext ctx)
    {
        var monitorSettings = ctx.Settings.RegisterSettings<MonitorSettingsProvider>();
        return monitorSettings.ShouldHideURLs(ctx.Guild.Id).ToString();
    }

    public void SetValue(ConfigurableContext ctx, string value)
    {
        var settings = ctx.Settings.RegisterSettings<MonitorSettingsProvider>();
        settings.SetShouldHideUrls(ctx.Guild.Id, bool.Parse(value));
    }
}