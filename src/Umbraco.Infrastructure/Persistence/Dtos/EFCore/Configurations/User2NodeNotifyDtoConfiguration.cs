using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class User2NodeNotifyDtoConfiguration : IEntityTypeConfiguration<User2NodeNotifyDto>
{
    public void Configure(EntityTypeBuilder<User2NodeNotifyDto> builder)
    {
        builder.ToTable(User2NodeNotifyDto.TableName);

        builder.HasKey(x => new { x.UserId, x.NodeId, x.Action })
            .HasName("PK_umbracoUser2NodeNotify");

        builder.Property(x => x.UserId)
            .HasColumnName("userId");

        builder.Property(x => x.NodeId)
            .HasColumnName("nodeId");

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(255);

        builder.HasOne<UserDto>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName($"FK_{User2NodeNotifyDto.TableName}_umbracoUser_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .HasConstraintName($"FK_{User2NodeNotifyDto.TableName}_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
