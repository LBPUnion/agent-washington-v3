using Discord;
using LBPUnion.AgentWashington.Core;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor.Commands; 

public class GetLatestTweetCommand : Command {
    public override string Name => "get-latest-tweet";

    public override string Description => "Gets the latest tweet from the linked twitter account.";

    protected override async Task OnHandle() {
        MonitorPlugin monitor = Modules.GetModule<MonitorPlugin>();

        Tweet[]? tweets = await monitor.TwitterClient.GetTweetsFromUserIdAsync(MonitorPlugin.TwitterAccountID.ToString(), new TweetSearchOptions {
            TweetOptions = Array.Empty<TweetOption>(),
            MediaOptions = Array.Empty<MediaOption>(),
            UserOptions = new[] {
                UserOption.Url,
            },
            Limit = 5,
        });
        if(tweets == null) {
            EmbedBuilder error = new();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot fetch tweets");
            error.WithDescription("An unknown error occurred while attempting to fetch tweets.");

            RespondWithEmbed(error.Build());
        }
        else {
            Tweet tweet = tweets[0];
            
            EmbedBuilder embed = new();
            embed.WithColor(new Color(0, 140, 255));
            embed.WithTitle(tweet.Author.Name);
            embed.WithDescription(tweet.Text);

            RespondWithEmbed(embed.Build());
        }
    }
}