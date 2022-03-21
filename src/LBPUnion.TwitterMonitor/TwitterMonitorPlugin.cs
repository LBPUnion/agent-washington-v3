using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.TwitterMonitor.Commands;
using LBPUnion.TwitterMonitor.Settings;
using LBPUnion.TwitterMonitor.Settings.Configurables;
using TwitterSharp.Client;

namespace LBPUnion.TwitterMonitor; 

[Plugin]
public class TwitterMonitorPlugin : BotModule {
    private CommandManager _commands;
    private SettingsManager _settings;
    private TwitterSettingsProvider _twitterSettings;

    internal TwitterClient TwitterClient;
    
    protected override void BeforeInit() {
        Logger.Log("It's time to waste cycles and look for tweets... ...and I'm all out of cycles.");

        this._settings = Modules.GetModule<SettingsManager>(); 
        this._commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init() {
        Logger.Log("Registering settings group...");
        this._twitterSettings = _settings.RegisterSettings<TwitterSettingsProvider>();

        Logger.Log("Setting up twitter client...");
        if(this._twitterSettings.TryGetBearerToken(out string bearerToken)) {
            this.TwitterClient = new TwitterClient(bearerToken);
        }
        else {
            throw new Exception("Unable to find a valid bearer token to use for Twitter. Please set it in the configuration file.");
        }

        Logger.Log("Registering configurables...");
        _settings.RegisterConfigurable("twitter.userId", new TwitterUserIdConfigurable());

        Logger.Log("Registering commands...");
        this._commands.RegisterCommand<GetLatestTweetCommand>();
    }

    public ulong GetTwitterUserId() {
        if(this._twitterSettings.TryGetTwitterUserId(out ulong userId)) {
            return userId;
        }
        
        return 0;
    }
}