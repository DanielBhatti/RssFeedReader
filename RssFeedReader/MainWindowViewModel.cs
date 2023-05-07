using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RssFeedReader;

public class MainWindowViewModel : ViewModelBase
{
    private string AppDataPath { get; }
    private string FeedListPath { get; }

    public ObservableCollection<RssFeed> FullFeedCollection { get; }

    public ObservableCollection<RssFeed> DisplayedFeedCollection { get => GetFilteredFeedCollection(); }

    private string? _newUriFeedName;
    public string? NewUriFeedName
    {
        get => _newUriFeedName;
        set => this.RaiseAndSetIfChanged(ref _newUriFeedName, value);
    }

    private string _displayText = "";
    public string DisplayText
    {
        get => _displayText;
        set => this.RaiseAndSetIfChanged(ref _displayText, value);
    }

    private string _filterText = "";
    public string FilterText
    {
        get => _filterText;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterText, value);
            this.RaisePropertyChanged(nameof(DisplayedFeedCollection));
        }
    }

    private Article _selectedArticle;
    public Article SelectedArticle
    {
        get => _selectedArticle;
        set => this.RaiseAndSetIfChanged(ref _selectedArticle, value);
    }

    public ArticleViewModel ArticleViewModel { get; }

    public MainWindowViewModel()
    {
        AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(RssFeedReader));
        if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

        FeedListPath = Path.Combine(AppDataPath, "feeds.txt");
        if (!File.Exists(FeedListPath)) File.Create(FeedListPath).Close();

        FullFeedCollection = new();

        string json;
        using (StreamReader reader = new StreamReader(FeedListPath)) json = reader.ReadToEnd();

        List<string> feedUris;
        if (!String.IsNullOrEmpty(json)) feedUris = JsonSerializer.Deserialize<List<string>>(json) ?? new();
        else feedUris = new();

        foreach (string feedUri in feedUris) AddFeed(feedUri);

        ArticleViewModel = new ArticleViewModel();
    }

    public async void AddFeed(string feedUriCsv)
    {
        if (String.IsNullOrEmpty(feedUriCsv))
        {
            DisplayText = "Must provide URI for at least one RSS feed.";
            return;
        }
        foreach (string feedUri in feedUriCsv.Split(',').Select(f => f.Trim()))
        {
            try
            {
                // maybe also try adding /feed to the end of the URI
                if (FullFeedCollection.Select(f => f.FeedUri).Contains(feedUri))
                {
                    DisplayText = $"Feed with URI {feedUri} already exists.";
                    return;
                }
                FullFeedCollection.Add(new RssFeed(feedUri));
                SaveFeedList();
                DisplayText = $"Successfully added feed {feedUri} to list.";
            }
            catch (Exception e)
            {
                DisplayText = $"Failed to add feed with URI {feedUri} to list.\n\n{e}";
            }
        }

        // load articles here
    }

    public void DeleteFeed(string feedUri)
    {
        RssFeed? feed = FullFeedCollection.FirstOrDefault(f => f.FeedUri == feedUri);
        if (feed != null) FullFeedCollection.Remove(feed.Value);
    }

    public void SaveFeedList()
    {
        using (StreamWriter writer = new StreamWriter(FeedListPath))
        {
            string json = JsonSerializer.Serialize(FullFeedCollection.Select(f => f.FeedUri).ToList());
            writer.Write(json);
        }
    }

    public ObservableCollection<RssFeed> GetFilteredFeedCollection()
    {
        if (String.IsNullOrEmpty(FilterText)) return FullFeedCollection;

        //todo add regex mode
        Dictionary<RssFeed, int> feedWordFrequency = new();
        foreach (RssFeed feed in FullFeedCollection)
        {
            feedWordFrequency[feed] = 0;
            foreach (string filterWord in FilterText.Split(' '))
            {
                string trimmedFilterWord = filterWord.Trim();
                if (feed.Title.Contains(trimmedFilterWord)) feedWordFrequency[feed]++;
            }
        }
        return new ObservableCollection<RssFeed>(FullFeedCollection.Where(f => feedWordFrequency[f] > 0).OrderByDescending(f => feedWordFrequency[f]));
    }
}
