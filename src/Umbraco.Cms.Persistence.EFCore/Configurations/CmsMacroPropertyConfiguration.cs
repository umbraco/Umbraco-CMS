using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsMacroPropertyConfiguration : IEntityTypeConfiguration<CmsMacroProperty>
    {
        public void Configure(EntityTypeBuilder<CmsMacroProperty> builder)
        {
            builder.ToTable("cmsMacroProperty");

            builder.HasIndex(e => new { e.Macro, e.MacroPropertyAlias }, "IX_cmsMacroProperty_Alias").IsUnique();

            builder.HasIndex(e => e.UniquePropertyId, "IX_cmsMacroProperty_UniquePropertyId").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.EditorAlias)
                .HasMaxLength(255)
                .HasColumnName("editorAlias");
            builder.Property(e => e.Macro).HasColumnName("macro");
            builder.Property(e => e.MacroPropertyAlias)
                .HasMaxLength(50)
                .HasColumnName("macroPropertyAlias");
            builder.Property(e => e.MacroPropertyName)
                .HasMaxLength(255)
                .HasColumnName("macroPropertyName");
            builder.Property(e => e.MacroPropertySortOrder)
                .HasDefaultValueSql("('0')")
                .HasColumnName("macroPropertySortOrder");
            builder.Property(e => e.UniquePropertyId).HasColumnName("uniquePropertyId");

            builder.HasOne(d => d.MacroNavigation).WithMany(p => p.CmsMacroProperties)
                .HasForeignKey(d => d.Macro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsMacroProperty_cmsMacro_id");
        }
    }
}
