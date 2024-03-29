using Discord.WebSocket;
using LBPUnion.AgentWashington.Core;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Persistence;
using LBPUnion.AgentWashington.Core.Plugins;
using LBPUnion.AgentWashington.Core.Settings;
using LBPUnion.TwitterMonitor.Commands;
using LBPUnion.TwitterMonitor.Database;
using LBPUnion.TwitterMonitor.Settings;
using LBPUnion.TwitterMonitor.Settings.Configurables;
using LiteDB;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor;

[Plugin]
public class TwitterMonitorPlugin : BotModule
{
    private CommandManager _commands;
    private SettingsManager _settings;
    private DatabaseManager _database;
    private TwitterSettingsProvider _twitterSettings;

    internal TwitterClient TwitterClient;

    private const double _updateInterval = 30;
    private double _timeUntilNextUpdate = 0;

    protected override void BeforeInit()
    {
        Logger.Log("It's time to waste cycles and look for tweets... ...and I'm all out of cycles.");

        this._settings = Modules.GetModule<SettingsManager>();
        this._commands = Modules.GetModule<CommandManager>();
        this._database = Modules.GetModule<DatabaseManager>();
    }

    protected override void Init()
    {
        Logger.Log("Registering settings group...");
        this._twitterSettings = _settings.RegisterSettings<TwitterSettingsProvider>();

        Logger.Log("Setting up twitter client...");
        if (this._twitterSettings.TryGetBearerToken(out string bearerToken))
        {
            this.TwitterClient = new TwitterClient(bearerToken);
        }
        else
        {
            throw new Exception(
                "Unable to find a valid bearer token to use for Twitter. Please set it in the configuration file.");
        }

        Logger.Log("Registering configurables...");
        _settings.RegisterConfigurable("twitter.userId", new TwitterUserIdConfigurable());
        _settings.RegisterConfigurable("twitter.updateChannelId", new UpdateChannelIdConfigurable());

        Logger.Log("Registering commands...");
        this._commands.RegisterCommand<GetLatestTweetCommand>();
        this._commands.RegisterCommand<ShowTweetCommand>();
    }

    private ulong GetTwitterUserId(ulong guildId)
    {
        if (this._twitterSettings.TryGetTwitterUserId(guildId, out ulong userId))
        {
            return userId;
        }

        return 0;
    }

    private TweetSearchOptions GetSearchOptions(ulong? guildId = null)
    {
        TweetSearchOptions searchOptions = new()
        {
            TweetOptions = new[]
            {
                TweetOption.Attachments,
                TweetOption.Created_At,
                TweetOption.Entities,
            },
            MediaOptions = new[]
            {
                MediaOption.Url,
            },
            UserOptions = new[]
            {
                UserOption.Profile_Image_Url,
                UserOption.Url,
            },
        };

        // It's important to not update the latest ID here; the users are supposed to be able to run /get-latest-tweet
        // Latest ID updates are and should be handled in OnTick.
        if (guildId != null)
        {
            searchOptions.SinceId = GetLatestTweetId((ulong) guildId).ToString();
        }

        return searchOptions;
    }

    public async Task<Tweet[]?> FetchLatestTweets(ulong guildId, bool allowLastTweet = false)
    {
        Logger.Log("Fetching tweets for guildId " + guildId);

        Tweet[] results = null;
        var failed = false;
        
        try
        {
            results = await TwitterClient.GetTweetsFromUserIdAsync(GetTwitterUserId(guildId).ToString(),
                this.GetSearchOptions(guildId));
        }
        catch (Exception ex)
        {
            Logger.Log($"Couldn't fetch latest tweets for guild guild {guildId}: {ex.Message}", LogLevel.Warning);
            failed = true;
        }
        finally
        {
            if (results == null)
                results = Array.Empty<Tweet>();
        }

        if (results.Length == 0 && allowLastTweet && !failed)
        {
            
            var searchOptions = this.GetSearchOptions(guildId);
            var lastTweet = searchOptions.SinceId ?? string.Empty;

            searchOptions.SinceId = null;

            if (!string.IsNullOrWhiteSpace(lastTweet))
            {
                var tweet = await TwitterClient.GetTweetAsync(lastTweet, searchOptions);
                if (tweet != null)
                {
                    results = new[] { tweet };
                }
            }
        }

        return results;
    }

    public async Task<Tweet?> FetchSingleTweet(ulong tweetId)
    {
        Logger.Log("Fetching tweetId " + tweetId);

        return await this.TwitterClient.GetTweetAsync(tweetId.ToString(), this.GetSearchOptions());
    }

    private void UpdateLatestTweetId(ulong guildId, ulong latestTweetId)
    {
        this._database.OpenDatabase(db =>
        {
            ILiteCollection<LatestTweetForGuildData> collection =
                db.GetCollection<LatestTweetForGuildData>("latestTweetForGuildData");

            LatestTweetForGuildData? data = collection.FindOne(d => d.GuildId == guildId);
            if (data != null)
            {
                data.LatestTweetId = latestTweetId;

                collection.Update(data);
            }
            else
            {
                data = new LatestTweetForGuildData
                {
                    GuildId = guildId,
                };

                collection.Insert(data);
            }
        });
    }

    private ulong GetLatestTweetId(ulong guildId)
    {
        ulong twitterId = 0;

        this._database.OpenDatabase(db =>
        {
            ILiteCollection<LatestTweetForGuildData> collection =
                db.GetCollection<LatestTweetForGuildData>("latestTweetForGuildData");

            LatestTweetForGuildData? data = collection.FindOne(d => d.GuildId == guildId);

            if (data != null)
            {
                twitterId = data.LatestTweetId;
            }
        });

        return twitterId;
    }

    protected override void OnTick(UpdateInterval interval)
    {
        _timeUntilNextUpdate -= interval.DeltaTime.TotalSeconds;
        if (!(_timeUntilNextUpdate <= 0)) return;
        
        Logger.Log("It's time to check servers for tweets!");
        DiscordBot bot = Modules.GetModule<DiscordBot>();
        List<SocketGuild> socketGuilds = bot.GetGuilds().ToList();

        foreach (TwitterMonitorGuild guild in this._twitterSettings.GetGuilds())
        {
            if (guild.TwitterUserId == null || guild.UpdateChannelId == null) continue;

            Tweet[]? tweets = this.FetchLatestTweets(guild.GuildId).Result;
            if (tweets == null || tweets.Length < 1) continue;

            SocketGuild? socketGuild = socketGuilds.FirstOrDefault(g => g.Id == guild.GuildId);
            SocketTextChannel? channel =
                socketGuild?.TextChannels.FirstOrDefault(c => c.Id == guild.UpdateChannelId);

            foreach (Tweet tweet in tweets.Reverse())
            {
                // Reverse to put the list in oldest-first order
                channel?.SendMessageAsync(embed: tweet.ToEmbed())
                    .Wait(); // Ticks are not asynchronous, so this will have to do.
            }

            UpdateLatestTweetId(guild.GuildId, ulong.Parse(tweets[0].Id));
        }

        _timeUntilNextUpdate = _updateInterval;
    }
}