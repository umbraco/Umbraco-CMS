using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Adds SQL Server-specific included columns to <see cref="RedirectUrlDto"/> indexes.
/// </summary>
public class SqlServerRedirectUrlDtoModelCustomizer : IEFCoreModelCustomizer<RedirectUrlDto>
{
    public string? ProviderName => Constants.ProviderNames.SQLServer;

    public void Customize(EntityTypeBuilder<RedirectUrlDto> builder) =>
        builder.HasIndex(x => x.CreateDateUtc)
            .HasDatabaseName($"IX_{RedirectUrlDto.TableName}_culture_hash")
            .IncludeProperties(x => new { x.Culture, x.Url, x.UrlHash, x.ContentKey });
}
