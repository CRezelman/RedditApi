
namespace RedditApi.Models
{
    public class Ratings
    {
        public int[] Users { get; set; }

        public Ratings()
        {
            Users = Array.Empty<int>();
        }

        public int Count()
        {
            return Users.Length;
        }
    }
    
    public class Content
    {
        public int ContentId { get; set; }
        public int UserId { get; set; }
        public Ratings Upvotes { get; set; }
        public Ratings Downvotes { get; set; }
        public string Body { get; set; }
        public Content()
        {
            Upvotes = new Ratings();
            Downvotes = new Ratings();
            Body = String.Empty;
        }
    }

    public class Posts
    {
        public string Title { get; set; }
        public Content Content { get; set; }
        public Content[] Comments { get; set; }

        public Posts()
        {
            Title = String.Empty;
            Content = new Content();
            Comments = Array.Empty<Content>();
        }
    }
}