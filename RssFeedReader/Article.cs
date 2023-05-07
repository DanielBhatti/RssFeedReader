using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RssFeedReader;

public record struct Article
{
    public required string SiteTitle { get; init; }
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string? Author { get; init; }
    public required string FeedUri { get; init; }
    public required string Description { get; init; }
    public required DateTime? PublishingDate { get; init; }
    public required ICollection<string> Categories { get; init; }
    public required string Content { get; init; }

    public static string GetHtmlFromPage(string uri)
    {
        using (HttpClient client = new HttpClient())
        using (HttpResponseMessage response = client.GetAsync(uri).Result)
        using (HttpContent content = response.Content)
        {
            return content.ReadAsStringAsync().Result;
        }
    }
}
