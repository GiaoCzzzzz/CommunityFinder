using CommunityFinder.Models;
using CommunityFinder.Services;
using System.Collections.ObjectModel;

namespace CommunityFinder.Views
{
    public partial class ForumSearchResultsPage : ContentPage
    {
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private ObservableCollection<ForumPost> _results;

        public ForumSearchResultsPage(List<ForumPost> results, ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _forumService = forumService;
            _authService = authService;
            _results = new ObservableCollection<ForumPost>(results);
            ResultsCollection.ItemsSource = _results;
        }

        private async void OnPostSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is ForumPost post)
            {
                ResultsCollection.SelectedItem = null;
                await Navigation.PushAsync(new PostDetailPage(post, _forumService, _authService));
            }
        }
    }
}
