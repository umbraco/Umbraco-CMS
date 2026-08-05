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
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<UserGroup2GranularPermissionDto> builder) =>
        builder.HasIndex(x => x.UserGroupKey)
            .HasDatabaseName($"IX_{UserGroup2GranularPermissionDto.TableName}_UserGroupKey_UniqueId")
            .IncludeProperties(x => x.UniqueId);
}
