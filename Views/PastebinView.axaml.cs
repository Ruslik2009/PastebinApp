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
    public partial class PastebinView : UserControl
    {
        public PastebinView()
        {
            InitializeComponent();
        }
        

        private async void CreatePasteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            bool isPublic = PublicCheckBox.IsChecked ?? true;

            int daysValue = Convert.ToInt32((DeleteComboBox.SelectedItem as ComboBoxItem)?.Tag ?? "0");
            DateTime? deleteTime = null;
            if (daysValue > 0)
            {
                deleteTime = DateTime.Now.AddDays(daysValue);
            }
            string url = HashHelper.GenerateSha256( MainWindow.userId.ToString() + ContentBox.Text + DateTime.Now.ToString());

            await MainWindow.DB.AddPasteAsync(MainWindow.userId, ContentBox.Text, isPublic, deleteTime, url );
            UrlBox.Text = url;
        }

        private async void CopyButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string textToCopy = UrlBox.Text ?? string.Empty;
        
            if (string.IsNullOrWhiteSpace(textToCopy)) return;
        
            var topLevel = TopLevel.GetTopLevel(this);
            var clipboard = topLevel?.Clipboard;
        
            if (clipboard != null)
            {
                try 
                {
                    await clipboard.SetTextAsync(textToCopy);
                    // Console.WriteLine("Скопировано!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка буфера: {ex.Message}");
                }
            }
        }

        private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            NavigationHelper.NavigateTo(this, new PostsView());
        }
    }
}