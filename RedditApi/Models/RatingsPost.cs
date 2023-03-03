namespace RedditApi.Models;

public class RatingsPost
{
    public long Id { get; set; }
    public long IdPost { get; set; }
    public long IdUser { get; set; }
    public RatingsType Rating { get; set; } 
}

public class RatingsComments
{
    public long Id { get; set; }
    public long IdComment { get; set; }
    public long IdUser { get; set; }
    public RatingsType Rating { get; set; } 
}

public enum RatingsType
{
    None,
    Downvote,
    Upvote
}