using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Data.Sqlite;
namespace PastebinApp
{
    public partial class PostsView : UserControl
    {

        private ObservableCollection<PostDisplay> Posts = new();

        public PostsView()
        {
            InitializeComponent();
            PostsList.ItemsSource = Posts;
            this.Loaded += OnLoaded;
        }


        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadPostsAsync();
            
        }


        private async Task LoadPostsAsync()
        {
            
            try
            {
                var posts = await MainWindow.DB.GetPostsAsync();


                Posts.Clear();

                foreach (var post in posts)
                {
                    var stats = await MainWindow.DB.GetPostStatsAsync(post.Id, MainWindow.userId);

                    post.LikesCount = stats.likes;
                    post.DislikesCount = stats.dislikes;
                    post.UserReaction = stats.userReaction;

                    Posts.Add(post);
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке постов: {ex.Message}");
            }
        }



        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var post = button?.DataContext as PostDisplay;
    
            if (post == null) return;

            var result = await MainWindow.DB.ToggleReactionAsync(post.Id, MainWindow.userId, 1);

            post.LikesCount = result.likes;
            post.DislikesCount = result.dislikes;
            post.UserReaction = result.userReaction;
        }

        private async void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var post = button?.DataContext as PostDisplay;
    
            if (post == null) return;

    
            var result = await MainWindow.DB.ToggleReactionAsync(post.Id, MainWindow.userId, 2);

            post.LikesCount = result.likes;
            post.DislikesCount = result.dislikes;
            post.UserReaction = result.userReaction;
        }



        private async void SearchPost_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UrlTextBox.Text))
            {
                await LoadPostsAsync();
            }
            else
            {
                var post = await MainWindow.DB.SearchPasteAsync(UrlTextBox.Text);
                Posts.Clear();
                Posts.Add(post);
            }


        }

        private void CreatePost_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateTo(this, new PastebinView());
        }

        
    }
}