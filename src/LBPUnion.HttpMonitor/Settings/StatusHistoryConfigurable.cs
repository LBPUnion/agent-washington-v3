using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.HttpMonitor.Settings;

public class StatusHistoryConfigurable : IConfigurable
{
    public string GetValue(ConfigurableContext ctx)
    {
        var monitorSettings = ctx.Settings.RegisterSettings<MonitorSettingsProvider>();
        if (monitorSettings.TryGetStatusHistoryChannel(ctx.Guild.Id, out var channel))
        {
            return channel.ToString();
        }

        return null;
    }

    public void SetValue(ConfigurableContext ctx, string value)
    {
        if (ulong.TryParse(value, out var channelId))
        {
            var channel = ctx.Guild.GetTextChannel(channelId);
            if (channel == null && channelId != 0)
                throw new InvalidOperationException("That channel doesn't exist!");

            var settings = ctx.Settings.RegisterSettings<MonitorSettingsProvider>();
            settings.SetStatusHistoryChannel(ctx.Guild.Id, channelId);
        }
        else
        {
            throw new InvalidOperationException("Not a valid Channel ID.");
        }
    }
}