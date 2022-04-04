using LBPUnion.AgentWashington.Core.Settings;

namespace LBPUnion.TwitterMonitor.Settings.Configurables; 

public class TwitterUserIdConfigurable : IConfigurable {
    public string GetValue(ConfigurableContext ctx) {
        TwitterSettingsProvider twitterSettings = ctx.Settings.RegisterSettings<TwitterSettingsProvider>();
        if(twitterSettings.TryGetTwitterUserId(ctx.Guild.Id, out ulong userId)) {
            return userId.ToString();
        }

        return null;
    }
    public void SetValue(ConfigurableContext ctx, string value) {
        if(ulong.TryParse(value, out ulong userId)) {
            TwitterSettingsProvider twitterSettings = ctx.Settings.RegisterSettings<TwitterSettingsProvider>();
            twitterSettings.SetTwitterUserId(ctx.Guild.Id, userId);
        }
        else {
            throw new ArgumentException("The userId given was not a valid number.");
        }
    }
}