using CodeHollow.FeedReader;
using System;
using System.Collections.Generic;

namespace RssFeedReader
{
    public record struct Article
    {
        public string SiteTitle { get; init; }
        public string Id { get; init; }
        public string Title { get; init; }
        public string Author { get; init; }
        public string FeedUri { get; init; }
        public string SiteUri { get; init; }
        public string Description { get; init; }
        public DateTime? PublishingDate { get; init; }
        public ICollection<string> Categories { get; init; }
        public string Content { get; init; }

        public static Article FromCodeHollowFeedItem(FeedItem chFeedItem, string siteTitle, string feedUri) =>
             new Article()
             {
                 SiteTitle = siteTitle,
                 Id = chFeedItem.Id,
                 Title = chFeedItem.Title,
                 Author = chFeedItem.Author,
                 FeedUri = feedUri,
                 SiteUri = chFeedItem.Link,
                 Description = chFeedItem.Description,
                 PublishingDate = chFeedItem.PublishingDate,
                 Categories = chFeedItem.Categories,
                 Content = chFeedItem.Content
             };
    }
}
