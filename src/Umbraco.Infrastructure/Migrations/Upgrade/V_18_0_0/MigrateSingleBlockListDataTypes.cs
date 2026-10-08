using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Switches all block list data types configured for single block mode over to the single block property editor.
/// </summary>
/// <remarks>
/// Only the data types are migrated. Stored property values are left in their block list format, which the single
/// block property editor reads as-is and replaces with its own format the next time a value is saved.
/// </remarks>
public class MigrateSingleBlockListDataTypes : AsyncMigrationBase
{
    private const string SingleBlockPropertyEditorUiAlias = "Umb.PropertyEditorUi.BlockSingle";

    private readonly IDataTypeService _dataTypeService;
    private readonly IBlockEditorElementTypeCache _elementTypeCache;
    private readonly AppCaches _appCaches;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;
    private readonly ILogger<MigrateSingleBlockListDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSingleBlockListDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="elementTypeCache">Cache for block editor element types.</param>
    /// <param name="appCaches">Provides access to application-level caches.</param>
    /// <param name="dataTypeConfigurationCache">Cache for data type configurations.</param>
    /// <param name="logger">The logger.</param>
    public MigrateSingleBlockListDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        IBlockEditorElementTypeCache elementTypeCache,
        AppCaches appCaches,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        ILogger<MigrateSingleBlockListDataTypes> logger)
        : base(context)
    {
        _dataTypeService = dataTypeService;
        _elementTypeCache = elementTypeCache;
        _appCaches = appCaches;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task MigrateAsync()
    {
        IDataType[] singleBlockListDataTypes = (await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.BlockList))
            .Where(dataType => dataType.ConfigurationObject is BlockListConfiguration { UseSingleBlockMode: true, ValidationLimit.Max: 1 })
            .ToArray();

        if (singleBlockListDataTypes.Length == 0)
        {
            _logger.LogInformation("No block list data types are configured for single block mode, nothing to do.");
            return;
        }

        _logger.LogInformation(
            "Switching {Count} block list data types configured for single block mode to the single block property editor.",
            singleBlockListDataTypes.Length);

        foreach (IEnumerable<int> ids in singleBlockListDataTypes.Select(dataType => dataType.Id).InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Sql<ISqlContext> sql = Database.SqlContext.Sql()
                .Update<DataTypeDto>(u => u
                    .Set(x => x.EditorAlias, Constants.PropertyEditors.Aliases.SingleBlock)
                    .Set(x => x.EditorUiAlias, SingleBlockPropertyEditorUiAlias))
                .WhereIn<DataTypeDto>(x => x.NodeId, ids);

            await Database.ExecuteAsync(sql);
        }

        // the element type cache, and the isolated/runtime caches it is built from in the default implementation,
        // still describe the data types as they were before the update - as does the data type configuration cache,
        // which is backed by its own memory cache rather than the application caches
        _elementTypeCache.ClearAll();
        _appCaches.IsolatedCaches.ClearAllCaches();
        _appCaches.RuntimeCache.Clear();
        _dataTypeConfigurationCache.ClearCache(singleBlockListDataTypes.Select(dataType => dataType.Key));
        RebuildCache = true;
    }
}
