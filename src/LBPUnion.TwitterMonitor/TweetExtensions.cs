using Discord;
using TwitterSharp.Response.RMedia;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor; 

public static class TweetExtensions {
    public static Embed ToEmbed(this Tweet tweet) {
        string tweetUrl = $"https://twitter.com/{tweet.Author.Username}/status/{tweet.Id}";
        
        EmbedBuilder embed = new();
        
        embed.WithAuthor(tweet.Author.Name, tweet.Author.ProfileImageUrl, tweetUrl);
        embed.WithDescription(tweet.Text);

        if(tweet.CreatedAt != null) embed.WithTimestamp((DateTimeOffset)tweet.CreatedAt);
        else embed.WithCurrentTimestamp();

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if(!tweet.Text.StartsWith("RT")) embed.WithColor(new Color(0, 140, 255)); // Union blue as default
        else embed.WithColor(new Color(28, 192, 137)); // Green, retweet button color since y'know its a retweet lol

        if(tweet.Attachments != null && tweet.Attachments.Media.Length != 0) {
            embed.WithImageUrl(tweet.Attachments.Media[0].Url);
        }

        return embed.Build();
    }
}