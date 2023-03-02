using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class Comment
{
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdPost { get; set; }
    public string Body { get; set; } = string.Empty;
}