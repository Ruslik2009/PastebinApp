using Avalonia.Controls;

namespace PastebinApp
{
    public static class NavigationHelper
    {
        public static void NavigateTo(UserControl currentControl, UserControl targetView)
        {
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(currentControl);
            mainWindow.MainContent.Content = targetView;
        }
    }
}