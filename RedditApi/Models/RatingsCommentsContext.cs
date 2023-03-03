using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class RatingsCommentsContext : DbContext
{
    public RatingsCommentsContext(DbContextOptions<RatingsCommentsContext> options)
        : base(options)
    {
    }

    public DbSet<RatingsComments> Ratings { get; set; } = null!;
}