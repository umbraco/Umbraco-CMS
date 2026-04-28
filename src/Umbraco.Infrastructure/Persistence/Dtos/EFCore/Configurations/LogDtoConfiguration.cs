using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class LogDtoConfiguration : IEntityTypeConfiguration<LogDto>
{
    public void Configure(EntityTypeBuilder<LogDto> builder)
    {
        builder.ToTable(LogDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(LogDto.IdColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName(LogDto.UserIdColumnName);

        builder.Property(x => x.NodeId)
            .HasColumnName(LogDto.NodeIdColumnName);

        builder.Property(x => x.EntityType)
            .HasColumnName(LogDto.EntityTypeColumnName)
            .HasMaxLength(50);

        builder.Property(x => x.Datestamp)
            .HasColumnName(LogDto.DatestampColumnName);

        builder.Property(x => x.Header)
            .HasColumnName(LogDto.HeaderColumnName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasColumnName(LogDto.CommentColumnName)
            .HasMaxLength(4000);

        builder.Property(x => x.Parameters)
            .HasColumnName(LogDto.ParametersColumnName)
            .HasMaxLength(4000);

        // IX_umbracoLog (non-clustered index on NodeId)
        builder.HasIndex(x => x.NodeId)
            .HasDatabaseName($"IX_{LogDto.TableName}");

        // IX_umbracoLog_datestamp (compound: Datestamp, userId, NodeId)
        builder.HasIndex(x => new { x.Datestamp, x.UserId, x.NodeId })
            .HasDatabaseName($"IX_{LogDto.TableName}_datestamp");

        // IX_umbracoLog_datestamp_logheader (compound: Datestamp, logHeader)
        builder.HasIndex(x => new { x.Datestamp, x.Header })
            .HasDatabaseName($"IX_{LogDto.TableName}_datestamp_logheader");
    }
}
