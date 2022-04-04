using Discord;
using LBPUnion.AgentWashington.Core;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor.Commands; 

public class ShowTweetCommand : Command {
    public override string Name => "show-tweet";

    public override string Description => "Embeds a tweet from a tweetId.";

    public override IEnumerable<Option> Options {
        get {
            yield return new Option("tweet", OptionType.String, "ID/URL of the tweet you want to embed", true);
        }
    }

    protected override async Task OnHandle() {
        TwitterMonitorPlugin twitterMonitor = Modules.GetModule<TwitterMonitorPlugin>();
        string tweetIdStr = GetArgument<string>("tweet");

        // Try to parse argument as a normal ulong
        if(!ulong.TryParse(tweetIdStr, out ulong tweetId)) {
            // If not, try reading it from a URL.
            bool gotIdFromUrl = false;
            
            try {
                Uri tweetUrl = new(tweetIdStr);
                tweetIdStr = tweetUrl.AbsolutePath.Substring(tweetUrl.AbsolutePath.LastIndexOf('/') + 1);

                tweetId = ulong.Parse(tweetIdStr);
                gotIdFromUrl = true;
            }
            catch {
                // ignored
            }

            // If not, then fail.
            if(!gotIdFromUrl) {
                EmbedBuilder error = new();
                error.WithColor(Color.Red);
                error.WithTitle("Invalid input");
                error.WithDescription("The tweet ID must be a valid ulong or tweet url.");

                RespondWithEmbed(error.Build());
                return;
            }
        }

        Tweet? tweet = await twitterMonitor.FetchSingleTweet(tweetId);

        if(tweet == null) {
            EmbedBuilder error = new();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot fetch tweets");
            error.WithDescription("An unknown error occurred while attempting to fetch that tweet.");

            RespondWithEmbed(error.Build());
        }
        else {
            RespondWithEmbed(tweet.ToEmbed());
        }
    }
}