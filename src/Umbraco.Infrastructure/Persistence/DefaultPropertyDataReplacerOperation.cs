using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Implements the <see cref="IPropertyDataReplacerOperation"/> as a default using database provider agnostic methods.
/// </summary>
internal class DefaultPropertyDataReplacerOperation : IPropertyDataReplacerOperation
{
    /// <inheritdoc/>
    public string? ProviderName => null;

    /// <inheritdoc/>
    public void ReplacePropertyData(IUmbracoDatabase database, int versionId, IEnumerable<PropertyDataDto> propertyDataDtos)
    {
        // Replace the property data.
        // Lookup the data to update with a UPDLOCK (using ForUpdate()) this is because we need to be atomic
        // and handle DB concurrency. Doing a clear and then re-insert is prone to concurrency issues.
        Sql<ISqlContext> propDataSql = database.SqlContext.Sql().Select("*").From<PropertyDataDto>().Where<PropertyDataDto>(x => x.VersionId == versionId).ForUpdate();
        List<PropertyDataDto>? existingPropData = database.Fetch<PropertyDataDto>(propDataSql);
        var propertyTypeToPropertyData = new Dictionary<(int propertyTypeId, int versionId, int? languageId, string? segment), PropertyDataDto>();
        var existingPropDataIds = new List<int>();
        foreach (PropertyDataDto? p in existingPropData)
        {
            existingPropDataIds.Add(p.Id);
            propertyTypeToPropertyData[(p.PropertyTypeId, p.VersionId, p.LanguageId, p.Segment)] = p;
        }

        var toUpdate = new List<PropertyDataDto>();
        var toInsert = new List<PropertyDataDto>();
        foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
        {
            // Check if this already exists and update, else insert a new one
            if (propertyTypeToPropertyData.TryGetValue((propertyDataDto.PropertyTypeId, propertyDataDto.VersionId, propertyDataDto.LanguageId, propertyDataDto.Segment), out PropertyDataDto? propData))
            {
                propertyDataDto.Id = propData.Id;
                toUpdate.Add(propertyDataDto);
            }
            else
            {
                toInsert.Add(propertyDataDto);
            }

            // track which ones have been processed
            existingPropDataIds.Remove(propertyDataDto.Id);
        }

        if (toUpdate.Count > 0)
        {
            var updateBatch = toUpdate
                .Select(x => UpdateBatch.For(x))
                .ToList();
            database.UpdateBatch(updateBatch, new BatchOptions { BatchSize = 100 });
        }

        if (toInsert.Count > 0)
        {
            database.InsertBulk(toInsert);
        }

        // For any remaining that haven't been processed they need to be deleted
        if (existingPropDataIds.Count > 0)
        {
            database.Execute(database.SqlContext.Sql().Delete<PropertyDataDto>().WhereIn<PropertyDataDto>(x => x.Id, existingPropDataIds));
        }
    }
}
