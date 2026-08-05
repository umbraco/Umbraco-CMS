using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class UserGroup2GranularPermissionDtoConfiguration : IEntityTypeConfiguration<UserGroup2GranularPermissionDto>
{
    public void Configure(EntityTypeBuilder<UserGroup2GranularPermissionDto> builder)
    {
        builder.ToTable(UserGroup2GranularPermissionDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName("PK_umbracoUserGroup2GranularPermissionDto");

        builder.Property(x => x.Id)
            .HasColumnName(UserGroup2GranularPermissionDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserGroupKey)
            .HasColumnName(UserGroup2GranularPermissionDto.UserGroupKeyColumnName)
            .IsRequired();

        builder.Property(x => x.UniqueId)
            .HasColumnName(UserGroup2GranularPermissionDto.UniqueIdColumnName);

        builder.Property(x => x.Permission)
            .HasColumnName(UserGroup2GranularPermissionDto.PermissionColumnName)
            .IsRequired();

        builder.Property(x => x.Context)
            .HasColumnName(UserGroup2GranularPermissionDto.ContextColumnName)
            .IsRequired();

        // FK: uniqueId -> umbracoNode.uniqueId. No relationship is modeled for userGroupKey since
        // UserGroupDto has not been ported to EF Core yet — the DB-level FK to umbracoUserGroup.key
        // still exists (created by NPoco), it's just invisible to this model.
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.UniqueId)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoUserGroup2GranularPermissionDto_UserGroupKey_UniqueId
        // Note: SQL Server included columns are added by SqlServerUserGroup2GranularPermissionDtoModelCustomizer.
        builder.HasIndex(x => x.UserGroupKey)
            .HasDatabaseName($"IX_{UserGroup2GranularPermissionDto.TableName}_UserGroupKey_UniqueId");

        // IX_umbracoUserGroup2GranularPermissionDto_UniqueId
        builder.HasIndex(x => x.UniqueId)
            .HasDatabaseName($"IX_{UserGroup2GranularPermissionDto.TableName}_UniqueId");
    }
}
