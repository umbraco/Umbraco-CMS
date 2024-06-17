using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class BlocksInRichTextTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    public BlocksInRichTextTelemetryProvider(IDataTypeService dataTypeService, IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _dataTypeService = dataTypeService;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IDataType> richTextDataTypes = _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText).GetAwaiter().GetResult().ToArray();
        int registeredBlocks = 0;
        yield return new UsageInformation(Constants.Telemetry.RichTextEditorCount, richTextDataTypes.Count());

        foreach (IDataType richTextDataType in richTextDataTypes)
        {
            RichTextConfiguration? richTextConfiguration = _dataTypeConfigurationCache.GetConfigurationAs<RichTextConfiguration>(richTextDataType.Key);
            if (richTextConfiguration is null)
            {
                // Might be some custom data type, skip it
                continue;
            }

            if (richTextConfiguration.Blocks is null)
            {
                continue;
            }

            registeredBlocks += richTextConfiguration.Blocks.Length;
        }

        yield return new UsageInformation(Constants.Telemetry.RichTextBlockCount, registeredBlocks);
    }
}
