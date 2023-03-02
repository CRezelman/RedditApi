using RedditApi.Models;

namespace Utilities;

public class Utilities
{
    public static bool PostExists(PostsContext context, long id)
    {
        return (context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    
    public static bool CommentExists(CommentContext context, long id)
    {
        return (context.Comment?.Any(e => e.Id == id)).GetValueOrDefault();
    }
        
    public static void FetchComments(CommentContext context, List<Post> posts)
    {
        foreach (var post in posts)
        {
            var comments = context.Comment.Where(c => c.IdPost == post.Id);
            post.Comments = comments.ToList();
        }
    }
}