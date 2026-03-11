using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
namespace PastebinApp
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        
        private void tbSignUp_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            NavigationHelper.NavigateTo(this, new RegisterView());
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e) 
        {
            if (string.IsNullOrEmpty(UsernameBox.Text) || string.IsNullOrEmpty(PasswordBox.Text))
            {
                ErrorBlock.Text = "*Error. Fill in your datails!";
            }
            else
            {
                //(await MainWindow.DB.VerificationUserAsync(UsernameBox.Text, PasswordBox.Text))? NavigationHelper.NavigateTo(this, new PastebinView()) : ErrorBlock.Text = "*The password/user in not correct";

                if(await MainWindow.DB.VerificationUserAsync(UsernameBox.Text, PasswordBox.Text))
                {
                    MainWindow.userId = (int)await MainWindow.DB.SearchUserAsync(UsernameBox.Text)!;
                    NavigationHelper.NavigateTo(this, new PostsView());

                }
                else
                {
                    ErrorBlock.Text = "*The password/user in not correct!";
                }
                
            }
        }


    }
}