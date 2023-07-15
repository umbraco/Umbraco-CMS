using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsMacroConfiguration : IEntityTypeConfiguration<CmsMacro>
    {
        public void Configure(EntityTypeBuilder<CmsMacro> builder)
        {
            builder.ToTable("cmsMacro");

            builder.HasIndex(e => e.MacroAlias, "IX_cmsMacroPropertyAlias").IsUnique();

            builder.HasIndex(e => e.UniqueId, "IX_cmsMacro_UniqueId").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.MacroAlias)
                .HasMaxLength(255)
                .HasColumnName("macroAlias");
            builder.Property(e => e.MacroCacheByPage)
                .IsRequired()
                .HasDefaultValueSql("('1')")
                .HasColumnName("macroCacheByPage");
            builder.Property(e => e.MacroCachePersonalized)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("macroCachePersonalized");
            builder.Property(e => e.MacroDontRender)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("macroDontRender");
            builder.Property(e => e.MacroName)
                .HasMaxLength(255)
                .HasColumnName("macroName");
            builder.Property(e => e.MacroRefreshRate)
                .HasDefaultValueSql("('0')")
                .HasColumnName("macroRefreshRate");
            builder.Property(e => e.MacroSource)
                .HasMaxLength(255)
                .HasColumnName("macroSource");
            builder.Property(e => e.MacroType).HasColumnName("macroType");
            builder.Property(e => e.MacroUseInEditor)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("macroUseInEditor");
            builder.Property(e => e.UniqueId).HasColumnName("uniqueId");
        }
    }
}
