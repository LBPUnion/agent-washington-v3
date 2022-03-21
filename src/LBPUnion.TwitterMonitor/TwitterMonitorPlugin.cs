using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.TwitterMonitor.Commands;
using LBPUnion.TwitterMonitor.Settings;
using LBPUnion.TwitterMonitor.Settings.Configurables;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor; 

[Plugin]
public class TwitterMonitorPlugin : BotModule {
    private CommandManager _commands;
    private SettingsManager _settings;
    private TwitterSettingsProvider _twitterSettings;

    internal TwitterClient TwitterClient;

    private const double _updateInterval = 30;
    private double _timeUntilNextUpdate = 0;
    
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

    public ulong GetTwitterUserId(ulong guildId) {
        if(this._twitterSettings.TryGetTwitterUserId(guildId, out ulong userId)) {
            return userId;
        }
        
        return 0;
    }

    public async Task<Tweet[]?> FetchTweets(ulong guildId) {
        Logger.Log("Fetching tweets for guildId " + guildId);
        
        return await TwitterClient.GetTweetsFromUserIdAsync(GetTwitterUserId(guildId).ToString(), new TweetSearchOptions {
            TweetOptions = Array.Empty<TweetOption>(),
            MediaOptions = Array.Empty<MediaOption>(),
            UserOptions = new[] {
                UserOption.Url,
            },
            Limit = 5,
            // The minimum twitter accepts is 5, for some reason. If we ignore it, twitter fires back a 400 bad request.
            // The latest tweet is tweets[0].
        });
    }

    protected override void OnTick(UpdateInterval interval) {
        _timeUntilNextUpdate -= interval.DeltaTime.TotalSeconds;
        if(_timeUntilNextUpdate <= 0) {
            Logger.Log("It's time to check servers for tweets!");
            DiscordBot bot = Modules.GetModule<DiscordBot>();

            foreach(TwitterMonitorGuild guild in this._twitterSettings.GetGuilds()) {
                Tweet[]? tweets = this.FetchTweets(guild.GuildId).Result;
            }

//            bot.GetGuilds()

            _timeUntilNextUpdate = _updateInterval;
        }
    }
}