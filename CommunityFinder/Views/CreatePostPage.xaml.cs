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
            _isReturningFromHistory = true;
            var historyPage = new HistoryPage(_authService);
            
            // Subscribe to the history page's item selection
            historyPage.Disappearing += async (s, args) =>
            {
                // When returning from history page, check if navigation data was set
                await Task.Delay(100); // Small delay to ensure navigation completes
            };

            await Navigation.PushAsync(historyPage);
            
            // Show prompt to inform user
            await DisplayAlert("Select Link", "Please select a course or event from your history to link to this post", "OK");
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
