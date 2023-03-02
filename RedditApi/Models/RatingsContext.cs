using Microsoft.EntityFrameworkCore;

namespace RedditApi.Models;

public class RatingsContext : DbContext
{
    public RatingsContext(DbContextOptions<RatingsContext> options)
        : base(options)
    {
    }

    public DbSet<Ratings> Ratings { get; set; } = null!;
}