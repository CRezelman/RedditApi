
using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models
{
    public class PostNew
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class Post : PostNew 
    {
        public long IdUser { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<Ratings>? Ratings { get; set; }

        public Post()
        {
            Title = string.Empty;
            Body = string.Empty;
            Comments = new List<Comment>();
            Ratings = new List<Ratings>();
        }

        public Post(PostNew postNew)
        {
            Id = postNew.Id;
            Title = postNew.Title;
            Body = postNew.Body;
        }
    }

    public class PostQuery
    {
        public int IdUser { get; set; }
        public int IdUserRatings { get; set; }
        public RatingsType Ratings { get; set; }

        public PostQuery()
        {
            IdUser = 0;
            IdUserRatings = 0;
            Ratings = RatingsType.None;
        }
    }
    
}