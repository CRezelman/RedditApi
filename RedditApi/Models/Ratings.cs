namespace RedditApi.Models;

public class Ratings
{
    public long Id { get; set; }
    public long IdPost { get; set; }
    public long IdUser { get; set; }
    public RatingsType Rating { get; set; } 
}

public enum RatingsType
{
    None,
    Downvote,
    Upvote
}