using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class RelationDtoConfiguration : IEntityTypeConfiguration<RelationDto>
{
    public void Configure(EntityTypeBuilder<RelationDto> builder)
    {
        builder.ToTable(RelationDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(RelationDto.IdColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ParentId)
            .HasColumnName(RelationDto.ParentIdColumnName);

        builder.Property(x => x.ChildId)
            .HasColumnName(RelationDto.ChildIdColumnName);

        builder.Property(x => x.RelationType)
            .HasColumnName(RelationDto.RelationTypeColumnName);

        builder.Property(x => x.Datetime)
            .HasColumnName(RelationDto.DatetimeColumnName);

        builder.Property(x => x.Comment)
            .HasColumnName(RelationDto.CommentColumnName)
            .HasMaxLength(1000);

        // IX_umbracoRelation_parentChildType (unique on parentId, childId, relType)
        builder.HasIndex(x => new { x.ParentId, x.ChildId, x.RelationType })
            .IsUnique()
            .HasDatabaseName($"IX_{RelationDto.TableName}_parentChildType");

        // FK_umbracoRelation_umbracoNode (ParentId -> NodeDto.NodeId)
        builder.HasOne(x => x.ParentNode)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{RelationDto.TableName}_{NodeDto.TableName}");

        // FK_umbracoRelation_umbracoNode1 (ChildId -> NodeDto.NodeId)
        builder.HasOne(x => x.ChildNode)
            .WithMany()
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{RelationDto.TableName}_{NodeDto.TableName}1");

        // FK to RelationType
        builder.HasOne(x => x.RelationTypeDto)
            .WithMany()
            .HasForeignKey(x => x.RelationType)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
