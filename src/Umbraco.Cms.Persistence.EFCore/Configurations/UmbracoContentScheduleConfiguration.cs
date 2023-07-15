using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoContentScheduleConfiguration : IEntityTypeConfiguration<UmbracoContentSchedule>
    {
        public void Configure(EntityTypeBuilder<UmbracoContentSchedule> builder)
        {
            builder.ToTable("umbracoContentSchedule");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            builder.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");

            builder.HasOne(d => d.Language).WithMany(p => p.UmbracoContentSchedules)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK_umbracoContentSchedule_umbracoLanguage_id");

            builder.HasOne(d => d.Node).WithMany(p => p.UmbracoContentSchedules)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
