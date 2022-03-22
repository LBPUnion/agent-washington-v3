using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.TwitterMonitor.Settings.Configurables; 

public class UpdateChannelIdConfigurable : IConfigurable {
    public string GetValue(ConfigurableContext ctx) {
        TwitterSettingsProvider twitterSettings = ctx.Settings.RegisterSettings<TwitterSettingsProvider>();
        if(twitterSettings.TryGetUpdateChannelId(ctx.Guild.Id, out ulong channelId)) {
            return channelId.ToString();
        }

        return null;
    }
    public void SetValue(ConfigurableContext ctx, string value) {
        if(ulong.TryParse(value, out ulong channelId)) {
            TwitterSettingsProvider twitterSettings = ctx.Settings.RegisterSettings<TwitterSettingsProvider>();
            twitterSettings.SetUpdateChannelId(ctx.Guild.Id, channelId);
        }
        else {
            throw new ArgumentException("The channelId given was not a valid number.");
        }
    }
}