using CodeHollow.FeedReader;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace RssFeedReader
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string AppDataPath { get; }
        private string FeedListPath { get; }

        public ObservableCollection<RssFeed> FeedCollection { get; }

        private string _newUriFeedName;
        public string NewUriFeedName
        {
            get => _newUriFeedName;
            set => this.RaiseAndSetIfChanged(ref _newUriFeedName, value);
        }

        private string _displayText;
        public string DisplayText
        {
            get => _displayText;
            set => this.RaiseAndSetIfChanged(ref _displayText, value);
        }

        public MainWindowViewModel()
        {
            AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(RssFeedReader));
            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

            FeedListPath = Path.Combine(AppDataPath, "feeds.txt");
            if (!File.Exists(FeedListPath)) File.Create(FeedListPath);

            string json;
            using (StreamReader reader = new StreamReader(FeedListPath))
            {
                json = reader.ReadToEnd();
            }
            if (!String.IsNullOrEmpty(json)) FeedCollection = JsonSerializer.Deserialize<ObservableCollection<RssFeed>>(json) ?? new();
            else FeedCollection = new();
        }

        public async void AddFeed(string feedUri)
        {
            try
            {
                Feed feed = await FeedReader.ReadAsync(feedUri);
                FeedCollection.Add(RssFeed.FromCodeHollowFeedReader(feed));
                SaveFeedList();
                DisplayText = $"Successfully added feed {feed.Title} to list.";
            }
            catch (Exception e)
            {
                DisplayText = $"Failed to add feed with URI {feedUri} to list.\n\n{e}";
            }
        }

        public void DeleteFeed(string feedUri)
        {

        }

        public void SaveFeedList()
        {
            using (StreamWriter writer = new StreamWriter(FeedListPath))
            {
                string json = JsonSerializer.Serialize(FeedCollection);
                writer.Write(json);
            }
        }
    }
}
