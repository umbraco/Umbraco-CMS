using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class AccessDtoConfiguration : IEntityTypeConfiguration<AccessDto>
{
    public void Configure(EntityTypeBuilder<AccessDto> builder)
    {
        builder.ToTable(AccessDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName($"PK_{AccessDto.TableName}");

        builder.Property(x => x.Id)
            .HasColumnName(AccessDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.NodeId)
            .HasColumnName("nodeId");

        builder.Property(x => x.LoginNodeId)
            .HasColumnName("loginNodeId");

        builder.Property(x => x.NoAccessNodeId)
            .HasColumnName("noAccessNodeId");

        builder.Property(x => x.CreateDate)
            .HasColumnName("createDate");

        builder.Property(x => x.UpdateDate)
            .HasColumnName("updateDate");

        // IX_umbracoAccess_nodeId — unique index on NodeId (mirrors NPoco DTO).
        builder.HasIndex(x => x.NodeId)
            .IsUnique()
            .HasDatabaseName($"IX_{AccessDto.TableName}_nodeId");

        builder
            .HasMany(x => x.Rules)
            .WithOne(x => x.Access)
            .HasForeignKey(x => x.AccessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .HasConstraintName("FK_umbracoAccess_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.LoginNodeId)
            .HasConstraintName("FK_umbracoAccess_umbracoNode_id1")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NoAccessNodeId)
            .HasConstraintName("FK_umbracoAccess_umbracoNode_id2")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
