using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class UserGroupDtoConfiguration : IEntityTypeConfiguration<UserGroupDto>
{
    public void Configure(EntityTypeBuilder<UserGroupDto> builder)
    {
        builder.ToTable(UserGroupDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName($"PK_{UserGroupDto.TableName}");

        builder.Property(x => x.Id)
            .HasColumnName(UserGroupDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Key)
            .HasColumnName(UserGroupDto.KeyColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName("userGroupAlias")
            .HasMaxLength(200);

        builder.Property(x => x.Name)
            .HasColumnName("userGroupName")
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.DefaultPermissions)
            .HasColumnName("userGroupDefaultPermissions")
            .HasMaxLength(50);

        builder.Property(x => x.CreateDate)
            .HasColumnName("createDate");

        builder.Property(x => x.UpdateDate)
            .HasColumnName("updateDate");

        builder.Property(x => x.Icon)
            .HasColumnName("icon");

        builder.Property(x => x.HasAccessToAllLanguages)
            .HasColumnName("hasAccessToAllLanguages");

        builder.Property(x => x.StartContentId)
            .HasColumnName("startContentId");

        builder.Property(x => x.StartMediaId)
            .HasColumnName("startMediaId");

        builder.Property(x => x.StartElementId)
            .HasColumnName("startElementId");

        // IX_umbracoUserGroup_userGroupKey
        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName($"IX_{UserGroupDto.TableName}_userGroupKey");

        // IX_umbracoUserGroup_userGroupAlias
        builder.HasIndex(x => x.Alias)
            .IsUnique()
            .HasDatabaseName($"IX_{UserGroupDto.TableName}_userGroupAlias");

        // IX_umbracoUserGroup_userGroupName
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName($"IX_{UserGroupDto.TableName}_userGroupName");

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.StartContentId)
            .HasConstraintName("FK_startContentId_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.StartMediaId)
            .HasConstraintName("FK_startMediaId_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.StartElementId)
            .HasConstraintName("FK_startElementId_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
