using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class CommentNew
{
    public long Id { get; set; }
    public long IdPost { get; set; }
    public string Body { get; set; } = string.Empty;
}
public class Comment : CommentNew
{
    public long IdUser { get; set; }
    public List<RatingsComments>? Ratings { get; set; }

    public Comment()
    {
        Body = string.Empty;
        Ratings = new List<RatingsComments>();
    }

    public Comment(CommentNew commentNew)
    {
        Id = commentNew.Id;
        IdPost = commentNew.IdPost;
        Body = commentNew.Body;
    }
    
}

public class CommentQuery
{
    public int IdUser { get; set; }
    public int IdPost { get; set; }

    public CommentQuery()
    {
        IdUser = 0;
        IdPost = 0;
    }
}