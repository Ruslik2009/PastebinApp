using CommunityToolkit.Mvvm.ComponentModel;
using System;  

namespace PastebinApp;

public partial class UserProfile : ObservableObject
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    [ObservableProperty]
    private int totalLikes;
    
    [ObservableProperty]
    private int totalDislikes;
    
    [ObservableProperty]
    private int postsCount;
}