using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityFinder.Models;
using CommunityFinder.Services;

namespace CommunityFinder.ViewModels
{
    // 课程详情页的 ViewModel
    public class CourseDetailViewModel : INotifyPropertyChanged
    {
        private readonly CourseDetailService _service = new();

        private CourseDetail? _detail;
        public CourseDetail? Detail { get => _detail; set { _detail = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string? _error;
        public string? Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public async Task LoadAsync(string detailUrl)
        {
            if (IsBusy) return;
            IsBusy = true;
            Error = null;
            try
            {
                Detail = await _service.GetCourseDetailAsync(detailUrl);
                if (Detail == null)
                    Error = "Failed to load course detail.";
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally { IsBusy = false; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
