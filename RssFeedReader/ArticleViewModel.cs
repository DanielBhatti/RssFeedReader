using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RssFeedReader
{
    public class ArticleViewModel : ViewModelBase
    {
        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                this.RaiseAndSetIfChanged(ref _filterText, value);
                this.RaisePropertyChanged(nameof(DisplayedArticleCollection));
            }
        }

        private string AppDataPath { get; }
        private string FeedListPath { get; }

        public ObservableCollection<Article> FullArticleCollection { get; }
        public ObservableCollection<Article> DisplayedArticleCollection { get => GetDisplayedCollection(FullArticleCollection, _filterText); }

        private Article _selectedArticle;
        public Article SelectedArticle
        {
            get => _selectedArticle;
            set => this.RaiseAndSetIfChanged(ref _selectedArticle, value);
        }

        public ArticleViewModel()
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
            if (!String.IsNullOrEmpty(json))
            {
                var feeds = JsonSerializer.Deserialize<ObservableCollection<RssFeed>>(json) ?? new();
                FullArticleCollection = new ObservableCollection<Article>(feeds.SelectMany(f => f.Articles));
            }
            else FullArticleCollection = new();
        }

        public ObservableCollection<Article> GetDisplayedCollection(ICollection<Article> articleCollection, string filterText)
        {
            if (String.IsNullOrEmpty(filterText)) return new ObservableCollection<Article>(articleCollection.OrderByDescending(a => a.PublishingDate));

            //todo add regex mode
            Dictionary<Article, int> articleWordFrequency = new();
            foreach (Article article in articleCollection)
            {
                articleWordFrequency[article] = 0;
                foreach (string filterWord in filterText.Split(' '))
                {
                    string trimmedFilterWord = filterWord.Trim();
                    if (article.Title.Contains(trimmedFilterWord)) articleWordFrequency[article]++;
                    if (article.Content.Contains(trimmedFilterWord)) articleWordFrequency[article]++;
                }
            }
            return new ObservableCollection<Article>(articleCollection.Where(f => articleWordFrequency[f] > 0).OrderByDescending(f => articleWordFrequency[f]));
        }
    }
}
