using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class User2NodeNotifyDtoConfiguration : IEntityTypeConfiguration<User2NodeNotifyDto>
{
    public void Configure(EntityTypeBuilder<User2NodeNotifyDto> builder)
    {
        builder.ToTable(User2NodeNotifyDto.TableName);

        builder.HasKey(x => new { x.UserId, x.NodeId, x.Action })
            .HasName("PK_umbracoUser2NodeNotify");

        builder.Property(x => x.UserId)
            .HasColumnName(User2NodeNotifyDto.UserIdColumnName)
            .IsRequired();

        builder.Property(x => x.NodeId)
            .HasColumnName(User2NodeNotifyDto.NodeIdColumnName)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName(User2NodeNotifyDto.ActionColumnName)
            .HasMaxLength(255)
            .IsRequired();

        // FK to umbracoNode (NodeDto is in EFCore model)
        builder.HasOne(x => x.NodeDto)
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // TODO: Configure FK to umbracoUser when UserDto is migrated to EFCore
    }
}
