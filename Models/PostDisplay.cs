using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;


namespace PastebinApp;

public partial class PostDisplay : ObservableObject
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime Created { get; set; }

    [ObservableProperty]
    private int likesCount;

    [ObservableProperty]
    private int dislikesCount;

    [ObservableProperty]
    private int? userReaction;

}