namespace RedditApi.Models;

public class User
{
    public long UserId { get; set; }
    public string Username { get; set; } = String.Empty;
    public string PasswordHash { get; set; } = String.Empty;

}