using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Configures SQL Server-specific clustered index behavior for <see cref="DocumentUrlAliasDto"/>.
/// The primary key is non-clustered.
/// </summary>
public class SqlServerDocumentUrlAliasDtoModelCustomizer : IEFCoreModelCustomizer<DocumentUrlAliasDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<DocumentUrlAliasDto> builder) =>
        builder.HasKey(x => x.Id)
            .IsClustered(false);
}
