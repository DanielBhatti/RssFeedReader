using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Diagnostics.CodeAnalysis;

namespace RssFeedReader;

public record struct RssFeed
{
    public required string Title { get; init; }
    public required string FeedUri { get; init; }
    public required string? SiteUri { get; init; }
    public required string? ImageUri { get; init; }
    public required List<Article> Articles { get; init; }

    [SetsRequiredMembers]
    public RssFeed(string feedUri)
    {
        if (!feedUri.EndsWith("/feed")) feedUri = feedUri.Trim('/') + "/feed";
        using (var reader = XmlReader.Create(feedUri, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse }))
        {
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            Title = feed.Title.Text;
            FeedUri = feedUri;
            SiteUri = feed.Links.FirstOrDefault()?.Uri.ToString();
            ImageUri = feed.ImageUrl.ToString();

            Articles = feed.Items.Select(item =>
            new Article()
            {
                SiteTitle = feed.Title.Text,
                Id = item.Id,
                Title = item.Title.Text,
                Author = item.Authors.FirstOrDefault()?.Name,
                FeedUri = feedUri,
                Description = item.Summary.Text,
                PublishingDate = item.PublishDate.ToLocalTime().Date,
                Categories = item.Categories.Select(c => c.Name).ToList(),
                Content = item.Content?.ToString() ?? Article.GetHtmlFromPage(item.Links.First().Uri.ToString()),
            })
                .ToList();
        }
    }
}
