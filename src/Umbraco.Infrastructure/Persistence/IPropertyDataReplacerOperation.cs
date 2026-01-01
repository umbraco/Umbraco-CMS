using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides replacement of property data for content versions.
/// </summary>
/// <remarks>
/// Database-specific implementations can use features like Table-Valued Parameters (SQL Server)
/// to perform the operation in a single round trip.
/// </remarks>
public interface IPropertyDataReplacerOperation
{
    /// <summary>
    /// Gets the database provider name this operation is specific to, or <c>null</c> for the default implementation.
    /// </summary>
    string? ProviderName { get; }

    /// <summary>
    /// Replaces all property data for the specified version IDs atomically.
    /// </summary>
    /// <param name="database">The database instance.</param>
    /// <param name="versionId">The version Id.</param>
    /// <param name="propertyDataDtos">The property data to save.</param>
    /// <remarks>
    /// This method will:
    /// 1. Lock existing rows for the affected version IDs.
    /// 2. Update existing property data where keys match (versionId, propertyTypeId, languageId, segment).
    /// 3. Insert new property data.
    /// 4. Delete property data that is no longer present.
    /// </remarks>
    void ReplacePropertyData(IUmbracoDatabase database, int versionId, IEnumerable<PropertyDataDto> propertyDataDtos);
}
