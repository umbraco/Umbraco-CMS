using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="UserGroup2GranularPermissionDto"/> indexes.
/// </summary>
public class SqlServerUserGroup2GranularPermissionDtoModelCustomizer : IEFCoreModelCustomizer<UserGroup2GranularPermissionDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<UserGroup2GranularPermissionDto> builder) =>
        builder.HasIndex(x => x.UserGroupKey)
            .HasDatabaseName("IX_umbracoUserGroup2GranularPermissionDto_UserGroupKey_UniqueId")
            .IncludeProperties(x => x.UniqueId);
}
