using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

public class TestUmbracoDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public TestUmbracoDbContext(DbContextOptions<TestUmbracoDbContext> options)
        : base(options)
    {
    }

    internal virtual DbSet<UmbracoLock> UmbracoLocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UmbracoLock>(entity =>
        {
            entity.ToTable("umbracoLock");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnName("name");

            entity.Property(e => e.Value).HasColumnName("value");
        });
    }
}
