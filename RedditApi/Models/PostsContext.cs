using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class PostsContext : DbContext
{
    public PostsContext(DbContextOptions<PostsContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Post { get; set; } = null!;
}