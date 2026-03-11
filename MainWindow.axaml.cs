using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text;

using Npgsql;
using Npgsql.Replication;
using Tmds.DBus.Protocol;

using Microsoft.Data.Sqlite;


namespace PastebinApp;

public partial class MainWindow : Window
{
    public static DatabaseService DB { get; private set; }= null!;
    public static int userId;
    
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new LoginView();

        //"Host=localhost;Database=pastebinapp_db;Username=postgres"
        //string connectionString = $"Data Source={dbPath};Cache=Shared;Mode=ReadWriteCreate;";

        string dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "app.db");
        string connectionString = $"Data Source={dbPath};Default Timeout=5;Cache=Shared;Mode=ReadWriteCreate;";
        DB = new DatabaseService(connectionString);
    }

    // Переключение mеnu
    private void ToggleMenu_Click(object sender, RoutedEventArgs e)
    {
        SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        if (SplitView.IsPaneOpen)
        {
            MenuIcon.Data = (StreamGeometry)this.FindResource("close_icon")!;
        }
        else
        {
            MenuIcon.Data = (StreamGeometry)this.FindResource("3line")!;
        }
    }


    private void HomeItem_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (MainContent.Content is LoginView || MainContent.Content is RegisterView )
        {
            //Close();
        }
        else
        {
            MainContent.Content = new PostsView();
        }
    }


    private void AccountItem_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (MainContent.Content is LoginView || MainContent.Content is RegisterView )
        {
            //Close();
        }
        else
        {
            MainContent.Content = new PostsView();
        }
    }



    private void CreatePostItem_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (MainContent.Content is LoginView || MainContent.Content is RegisterView )
        {
            //Close();
        }
        else
        {
            MainContent.Content = new PastebinView();
        }
    }


    private void ExitItem_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (MainContent.Content is LoginView || MainContent.Content is RegisterView )
        {
            Close();
        }
        else
        {
             MainContent.Content = new LoginView();
        }
    }

}