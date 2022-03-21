using Discord;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor; 

public static class TweetExtensions {
    public static Embed ToEmbed(this Tweet tweet) {
        EmbedBuilder embed = new();
        embed.WithColor(new Color(0, 140, 255));
        embed.WithTitle(tweet.Author.Name);
        embed.WithDescription(tweet.Text);

        return embed.Build();
    }
}