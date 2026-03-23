using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

/// <summary>
/// Used by the SingleBlockList Migration and its processors to avoid having to fetch (and thus lock)
/// data from the db multiple times during the migration.
/// </summary>
/// <remarks>Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21</remarks>
[Obsolete("Scheduled for removal in Umbraco 22.")]
public class SingleBlockListConfigurationCache
{
    private readonly IDataTypeService _dataTypeService;
    private readonly List<IDataType> _singleBlockListDataTypes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockListConfigurationCache"/> class.
    /// </summary>
    /// <param name="dataTypeService">Service for retrieving data type information.</param>
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

    /// <summary>
    /// Checks whether the block list property editor with the specified key is configured to use single block mode.
    /// </summary>
    /// <remarks>returns whether the passed in key belongs to a blocklist with UseSingleBlockMode set to true</remarks>
    /// <param name="key">The unique identifier of the block list data type.</param>
    /// <returns><c>true</c> if the block list is configured for single block mode; otherwise, <c>false</c>.</returns>
    public bool IsPropertyEditorBlockListConfiguredAsSingle(Guid key) =>
        _singleBlockListDataTypes.Any(dt => dt.Key == key);

    /// <summary>
    /// Gets a read-only collection of blocklist data types that have <c>UseSingleBlockMode</c> set to <c>true</c>.
    /// </summary>
    /// <remarks>The list of all blocklist data types that have UseSingleBlockMode set to true</remarks>
    public IEnumerable<IDataType> CachedDataTypes => _singleBlockListDataTypes.AsReadOnly();
}
