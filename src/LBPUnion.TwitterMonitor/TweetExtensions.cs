using System.Diagnostics;
using Discord;
using TwitterSharp.Response.Entity;
using TwitterSharp.Response.RMedia;
using TwitterSharp.Response.RTweet;

namespace LBPUnion.TwitterMonitor; 

public static class TweetExtensions {
    public static Embed ToEmbed(this Tweet tweet) {
        string tweetUrl = $"https://twitter.com/{tweet.Author.Username}/status/{tweet.Id}";
        
        EmbedBuilder embed = new();
        
        embed.WithAuthor(tweet.Author.Name, tweet.Author.ProfileImageUrl, tweetUrl);

        if(tweet.CreatedAt != null) embed.WithTimestamp((DateTimeOffset)tweet.CreatedAt);
        else embed.WithCurrentTimestamp();

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if(!tweet.Text.StartsWith("RT")) embed.WithColor(new Color(0, 140, 255)); // Union blue as default
        else embed.WithColor(new Color(28, 192, 137)); // Green, retweet button color since y'know its a retweet lol

        if(tweet.Attachments != null && tweet.Attachments.Media.Length != 0) {
            embed.WithImageUrl(tweet.Attachments.Media[0].Url);
        }

        string description = tweet.Text;

        if(tweet.Entities != null) {
            // Show full URLs instead of twitter shortened URLs
            foreach(EntityUrl url in tweet.Entities.Urls ?? Array.Empty<EntityUrl>()) {
                if(!url.DisplayUrl.Contains("pic.twitter.com")) { // Replace normal URLS
                    description = description.Replace(url.Url, url.ExpandedUrl);
                }
            }

            // Link to hashtags
            foreach(EntityTag hashtag in tweet.Entities.Hashtags ?? Array.Empty<EntityTag>()) {
                description = description.Replace('#' + hashtag.Tag, $"[#{hashtag.Tag}](https://twitter.com/hashtag/{hashtag.Tag}?src=hashtag_click)");
            }

            // Link to mentions
            // mention.Tag is blank for some reason, so this is disabled for now.
//        foreach(EntityTag mention in tweet.Entities.Mentions ?? Array.Empty<EntityTag>()) {
//            description = description.Replace('@' + mention.Tag, $"[@{mention.Tag}](https://twitter.com/{mention.Tag})");
//        }
        }

        embed.WithDescription(description);

        return embed.Build();
    }
}