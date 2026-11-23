using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;

/// <summary>
/// Removes the filter the SQL Server provider adds by convention to the unique <see cref="LanguageDto"/> index
/// over nullable columns, so the model matches the unfiltered index the database has.
/// </summary>
public class SqlServerLanguageDtoModelCustomizer : IEFCoreModelCustomizer<LanguageDto>
{
    public string? ProviderName => Constants.ProviderNames.EFCore.SQLServer;

    public void Customize(EntityTypeBuilder<LanguageDto> builder) =>
        builder.HasIndex(x => x.IsoCode)
            .IsUnique()
            .HasFilter(null);
}
