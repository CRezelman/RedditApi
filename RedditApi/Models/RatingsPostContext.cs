using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class RatingsPostContext : DbContext
{
    public RatingsPostContext(DbContextOptions<RatingsPostContext> options)
        : base(options)
    {
    }

    public DbSet<RatingsPost> RatingsPosts { get; set; } = null!;
}