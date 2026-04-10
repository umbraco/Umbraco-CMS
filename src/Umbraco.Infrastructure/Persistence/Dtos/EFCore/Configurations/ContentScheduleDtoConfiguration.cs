using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentScheduleDtoConfiguration : IEntityTypeConfiguration<ContentScheduleDto>
{
    public void Configure(EntityTypeBuilder<ContentScheduleDto> builder)
    {
        builder.ToTable(ContentScheduleDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ContentScheduleDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.NodeId)
            .HasColumnName(ContentScheduleDto.NodeIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(ContentScheduleDto.LanguageIdColumnName);

        builder.Property(x => x.Date)
            .HasColumnName(ContentScheduleDto.DateColumnName);

        builder.Property(x => x.Action)
            .HasColumnName(ContentScheduleDto.ActionColumnName);

        // FK: NodeId -> umbracoContent.nodeId
        builder.HasOne<ContentDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
