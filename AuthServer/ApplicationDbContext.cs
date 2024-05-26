using Microsoft.EntityFrameworkCore;

namespace AuthServer;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict();
        base.OnModelCreating(builder);
    }
}