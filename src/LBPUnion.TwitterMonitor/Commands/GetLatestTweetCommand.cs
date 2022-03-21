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
        TwitterMonitorPlugin twitterMonitor = Modules.GetModule<TwitterMonitorPlugin>();

        Tweet[]? tweets = await twitterMonitor.FetchTweets(Guild.Id);
        
        if(tweets == null || tweets.Length < 1) {
            EmbedBuilder error = new();
            error.WithColor(Color.Red);
            error.WithTitle("Cannot fetch tweets");
            error.WithDescription("An unknown error occurred while attempting to fetch tweets.");

            RespondWithEmbed(error.Build());
        }
        else {
            Tweet tweet = tweets[0];
            RespondWithEmbed(tweet.ToEmbed());
        }
    }
}