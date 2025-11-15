using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.Views
{
    public partial class CreatePostPage : ContentPage
    {
        private readonly ForumCategory _category;
        private readonly ForumService _forumService;
        private readonly AuthService _authService;
        private string _linkedCourseId;
        private string _linkedEventId;
        private bool _isReturningFromHistory = false;

        public CreatePostPage(ForumCategory category, ForumService forumService, AuthService authService)
        {
            InitializeComponent();
            _category = category;
            _forumService = forumService;
            _authService = authService;
            PostTypePicker.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (_isReturningFromHistory)
            {
                _isReturningFromHistory = false;
                // Check if any item was selected from history
                // This will be handled by the navigation result
            }
        }

        private async void OnAddLinkClicked(object sender, EventArgs e)
        {
            var historyPage = new HistoryPage(_authService, selectionMode: true);
            
            // Subscribe to the item selection event
            historyPage.ItemSelected += (s, item) =>
            {
                if (item.IsEvent)
                {
                    SetLinkedEvent(item.Id, item.Title);
                }
                else
                {
                    SetLinkedCourse(item.Id, item.Title);
                }
            };

            await Navigation.PushAsync(historyPage);
        }

        public void SetLinkedCourse(string courseId, string courseName)
        {
            _linkedCourseId = courseId;
            _linkedEventId = null;
            LinkedItemLabel.Text = $"📚 {courseName}";
            LinkedItemFrame.IsVisible = true;
        }

        public void SetLinkedEvent(string eventId, string eventName)
        {
            _linkedEventId = eventId;
            _linkedCourseId = null;
            LinkedItemLabel.Text = $"🎉 {eventName}";
            LinkedItemFrame.IsVisible = true;
        }

        private void OnRemoveLinkClicked(object sender, EventArgs e)
        {
            _linkedCourseId = null;
            _linkedEventId = null;
            LinkedItemFrame.IsVisible = false;
        }

        private async void OnCreatePostClicked(object sender, EventArgs e)
        {
            var topic = TopicEntry.Text?.Trim();
            var content = ContentEditor.Text?.Trim();
            var postType = PostTypePicker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(topic))
            {
                await DisplayAlert("Error", "Please enter a topic", "OK");
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                await DisplayAlert("Error", "Please enter content", "OK");
                return;
            }

            if (string.IsNullOrEmpty(postType))
            {
                await DisplayAlert("Error", "Please select a post type", "OK");
                return;
            }

            try
            {
                await _forumService.CreatePostAsync(
                    _category.Id, 
                    topic, 
                    content, 
                    postType,
                    _linkedCourseId,
                    _linkedEventId);

                await DisplayAlert("Success", "Post created successfully!", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to create post: {ex.Message}", "OK");
            }
        }
    }
}
