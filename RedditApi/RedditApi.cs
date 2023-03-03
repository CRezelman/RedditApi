using RedditApi.Models;

namespace Utilities;

public static class RedditApi
{
    public static bool PostExists(PostsContext context, long id)
    {
        return (context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    
    public static bool CommentExists(CommentContext context, long id)
    {
        return (context.Comment?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    
    public static bool RatingPostExists(RatingsPostContext context, long idPost)
    {
        return (context.RatingsPosts?.Any(e => e.IdPost == idPost)).GetValueOrDefault();
    }
    
    public static bool RatingCommentExists(RatingsCommentsContext context, long idComment)
    {
        return (context.RatingsComments?.Any(e => e.IdComment == idComment)).GetValueOrDefault();
    }
        
    public static void FetchComments(CommentContext context, RatingsCommentsContext commentContext, List<Post> posts)
    {
        foreach (var post in posts)
        {
            var comments = context.Comment.Where(c => c.IdPost == post.Id);
            post.Comments = comments.ToList();
            FetchCommentRatings(commentContext, post.Comments);
        }
    }        
    public static void FetchComments(CommentContext context, Post post)
    {
        var comments = context.Comment.Where(c => c.IdPost == post.Id);
        post.Comments = comments.ToList();
    }
    
    public static void FetchCommentRatings(RatingsCommentsContext commentContext, List<Comment> comments)
    {
        foreach (var comment in comments)
        {
            var ratings = commentContext.RatingsComments.Where(r => r.IdComment == comment.Id);
            comment.Ratings = ratings.ToList();
        }
    } 
    
    public static void FetchPostRatings(RatingsPostContext postContext, List<Post> posts)
    {
        foreach (var post in posts)
        {
            var ratings = postContext.RatingsPosts.Where(r => r.IdPost == post.Id);
            post.Ratings = ratings.ToList();
        }
    } 
    public static void FetchPostRatings(RatingsPostContext postContext, Post post)
    {
        var ratings = postContext.RatingsPosts.Where(r => r.IdPost == post.Id);
        post.Ratings = ratings.ToList();
    }
}