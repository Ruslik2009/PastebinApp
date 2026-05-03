using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Interactivity;

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

        private async void DeletePost_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int postId)
            {
                var confirmDialog = new Window
                {
                    Title = "Подтверждение",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock
                {
                    Text = "Вы уверены, что хотите удалить этот пост?",
                    Margin = new Avalonia.Thickness(20),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var stackPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 10, 0, 0)
                };

                var yesButton = new Button
                {
                    Content = "Да",
                    Margin = new Avalonia.Thickness(0, 0, 10, 0),
                    Background = Avalonia.Media.Brushes.Green,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(20, 5)
                };

                var noButton = new Button
                {
                    Content = "Нет",
                    Background = Avalonia.Media.Brushes.Gray,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(20, 5)
                };

                stackPanel.Children.Add(yesButton);
                stackPanel.Children.Add(noButton);

                var mainPanel = new StackPanel();
                mainPanel.Children.Add(textBlock);
                mainPanel.Children.Add(stackPanel);

                confirmDialog.Content = mainPanel;

                bool result = false;

                yesButton.Click += (s, args) =>
                {
                    result = true;
                    confirmDialog.Close();
                };

                noButton.Click += (s, args) =>
                {
                    result = false;
                    confirmDialog.Close();
                };

                await confirmDialog.ShowDialog((Avalonia.Visuals.Platform.IWindowImpl?)null);

                if (result)
                {
                    bool success = await MainWindow.DB.DeletePostAsync(postId, MainWindow.userId);
                    
                    if (success)
                    {
                        // Удаляем пост из коллекции
                        var postToRemove = _posts.FirstOrDefault(p => p.Id == postId);
                        if (postToRemove != null)
                        {
                            _posts.Remove(postToRemove);
                            
                            // Обновляем счетчик постов в профиле
                            if (_profile != null)
                            {
                                _profile.PostsCount = _posts.Count;
                            }
                        }
                    }
                    else
                    {
                        var errorWindow = new Window
                        {
                            Title = "Ошибка",
                            Width = 300,
                            Height = 150,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        
                        errorWindow.Content = new TextBlock
                        {
                            Text = "Не удалось удалить пост. Возможно, он уже был удален.",
                            Margin = new Avalonia.Thickness(20),
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        };
                        
                        await errorWindow.ShowDialog((Avalonia.Visuals.Platform.IWindowImpl?)null);
                    }
                }
            }
        }
    }
}
