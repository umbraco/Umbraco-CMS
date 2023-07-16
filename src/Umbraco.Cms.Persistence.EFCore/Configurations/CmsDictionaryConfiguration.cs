using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsDictionaryConfiguration : IEntityTypeConfiguration<CmsDictionary>
    {
        public void Configure(EntityTypeBuilder<CmsDictionary> builder)
        {
            builder.HasKey(e => e.Pk);

            builder.ToTable("cmsDictionary");

            builder.HasIndex(e => e.Parent, "IX_cmsDictionary_Parent");

            builder.HasIndex(e => e.Id, "IX_cmsDictionary_id").IsUnique();

            builder.HasIndex(e => e.Key, "IX_cmsDictionary_key").IsUnique();

            builder.Property(e => e.Pk).HasColumnName("pk");
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Key).HasColumnName("key");
            builder.Property(e => e.Parent).HasColumnName("parent");

            builder.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("FK_cmsDictionary_cmsDictionary_id");
        }
    }
}
