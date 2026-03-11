using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text;

using Microsoft.Data.Sqlite;
namespace PastebinApp
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }
        
        private void tbSignIn_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            NavigationHelper.NavigateTo(this, new LoginView());
        }  

        private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            //UsernameBox.Text.Lenght < 1  || PasswordBox.Text.Lenght < 3
            if (string.IsNullOrEmpty(UsernameBox.Text) || string.IsNullOrEmpty(PasswordBox.Text))
            {
                ErrorBlock.Text = "*The user/password is empty!";
            }
            else if (UsernameBox.Text.Length < 1  || PasswordBox.Text.Length < 3)
            {
                ErrorBlock.Text = "*The user/password is short!";
            }
            else if (await MainWindow.DB.SearchUserAsync(UsernameBox.Text) != null)
            {
                ErrorBlock.Text = "*Change your username. Please!";
            }
            else
            {
                if (PasswordBox.Text == ConfirmPasswordBox.Text)
                {
                    await MainWindow.DB.AddUserAsync(UsernameBox.Text, PasswordBox.Text);
                    MainWindow.userId = (int)await MainWindow.DB.SearchUserAsync(UsernameBox.Text);
                    NavigationHelper.NavigateTo(this, new PostsView());
                }
                else
                {
                    ErrorBlock.Text = "*Passwords are not the same!";
                }
            }
            
        }

    }
}