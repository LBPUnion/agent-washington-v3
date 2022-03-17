using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.TwitterMonitor.Commands;
using TwitterSharp.Client;

namespace LBPUnion.TwitterMonitor; 

[Plugin]
public class MonitorPlugin : BotModule {
    private CommandManager _commands;

    public const long TwitterAccountID = 2946409263;

    internal TwitterClient TwitterClient;
    
    protected override void BeforeInit() {
        Logger.Log("It's time to waste cycles and look for tweets... ...and I'm all out of cycles.");

        _commands = Modules.GetModule<CommandManager>();
    }

    protected override void Init() {
        this.TwitterClient
            = new TwitterClient("");
        
        this._commands.RegisterCommand<GetLatestTweetCommand>();
    }
}