using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PastebinApp
{
    public partial class UserProfileView : UserControl
    {
        private UserProfile? _profile;
        private ObservableCollection<PostDisplay> _posts = new();

        public UserProfileView()
        {
            InitializeComponent();
            PostsList.ItemsSource = _posts;
            this.Loaded += async (s, e) => await LoadUserProfileAsync();
        }

        private async Task LoadUserProfileAsync()
        {
            if (MainWindow.userId <= 0) return;

            // Загружаем профиль пользователя
            _profile = await MainWindow.DB.GetUserProfileAsync(MainWindow.userId);
            if (_profile == null) return;

            // Загружаем посты пользователя
            var userPosts = await MainWindow.DB.GetUserPostsAsync(MainWindow.userId);
            _posts.Clear();
            foreach (var post in userPosts)
            {
                _posts.Add(post);
            }

            // Загружаем общую статистику реакций
            var reactions = await MainWindow.DB.GetUserReactionsAsync(MainWindow.userId);

            // Обновляем свойства профиля
            _profile.PostsCount = _posts.Count;
            _profile.TotalLikes = reactions.totalLikes;
            _profile.TotalDislikes = reactions.totalDislikes;

            // Устанавливаем DataContext для привязки
            DataContext = _profile;
        }
    }
}
