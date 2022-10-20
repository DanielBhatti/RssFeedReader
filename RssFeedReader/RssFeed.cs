using CodeHollow.FeedReader;
using System.Collections.Generic;
using System.Linq;

namespace RssFeedReader;

public record struct RssFeed
{
    public string Title { get; init; }
    public string FeedUri { get; init; }
    public string SiteUri { get; init; }
    public string ImageUri { get; init; }
    public List<Article> Articles { get; init; }

    public static RssFeed FromCodeHollowFeedReader(Feed chFeed, string feedUri) =>
         new RssFeed()
         {
             Title = chFeed.Title,
             FeedUri = feedUri,
             SiteUri = chFeed.Link,
             ImageUri = chFeed.ImageUrl,
             Articles = chFeed.Items.Select(i => Article.FromCodeHollowFeedItem(i, chFeed.Title, feedUri)).ToList()
         };
}
