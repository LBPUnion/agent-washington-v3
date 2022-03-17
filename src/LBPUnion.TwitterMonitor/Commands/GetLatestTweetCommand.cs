using Discord;
using LBPUnion.AgentWashington.Core;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor.Commands; 

public class GetLatestTweetCommand : Command {
    protected override async Task OnHandle() {
        MonitorPlugin monitor = Modules.GetModule<MonitorPlugin>();

        Tweet[]? tweets = await monitor.TwitterClient.GetTweetsFromUserIdAsync(MonitorPlugin.TwitterAccountID.ToString(), new TweetSearchOptions() {
            Limit = 1,
        });
        if(tweets == null) {
            EmbedBuilder error = new();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot fetch tweets");
            error.WithDescription("An unknown error occurred while attempting to fetch tweets.");

            RespondWithEmbed(error.Build());
        }
        else {
            foreach(Tweet tweet in tweets) {
                EmbedBuilder embed = new();
                embed.WithColor(new Color(0, 140, 255));
                embed.WithTitle(tweet.Author.Name);
                embed.WithDescription(tweet.Text);

                RespondWithEmbed(embed.Build());
            }
        }
    }
}