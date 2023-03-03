using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class CommentContext : DbContext
{
    public CommentContext(DbContextOptions<CommentContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comment { get; set; } = null!;
}