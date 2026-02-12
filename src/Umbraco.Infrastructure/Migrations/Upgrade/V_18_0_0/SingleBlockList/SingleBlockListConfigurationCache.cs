using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

/// <summary>
/// Used by the SingleBlockList Migration and its processors to avoid having to fetch (and thus lock)
/// data from the db multiple times during the migration.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListConfigurationCache
{
    private readonly IDataTypeService _dataTypeService;
    private readonly List<IDataType> _singleBlockListDataTypes = new();

    public SingleBlockListConfigurationCache(IDataTypeService dataTypeService)
    {
        _dataTypeService = dataTypeService;
    }

    /// <summary>
    /// Populates a cache that holds all the property editor aliases that have a BlockList configuration with UseSingleBlockMode set to true.
    /// </summary>
    /// <returns> The number of blocklists with UseSingleBlockMode set to true.</returns>
    public async Task<int> Populate()
    {
        IEnumerable<IDataType> blockListDataTypes =
            await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.BlockList);

        foreach (IDataType dataType in blockListDataTypes)
        {
            if (dataType.ConfigurationObject is BlockListConfiguration
                {
                    UseSingleBlockMode: true, ValidationLimit.Max: 1
                })
            {
                _singleBlockListDataTypes.Add(dataType);
            }
        }

        return _singleBlockListDataTypes.Count;
    }

    // returns whether the passed in key belongs to a blocklist with UseSingleBlockMode set to true
    public bool IsPropertyEditorBlockListConfiguredAsSingle(Guid key) =>
        _singleBlockListDataTypes.Any(dt => dt.Key == key);

    // The list of all blocklist data types that have UseSingleBlockMode set to true
    public IEnumerable<IDataType> CachedDataTypes => _singleBlockListDataTypes.AsReadOnly();
}
