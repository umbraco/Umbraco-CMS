using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class BlocksInRichTextTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IDataTypeService _dataTypeService;

    public BlocksInRichTextTelemetryProvider(IDataTypeService dataTypeService)
    {
        _dataTypeService = dataTypeService;
    }

    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IDataType> richTextDataTypes = _dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.TinyMce).ToArray();
        int registeredBlocks = 0;
        yield return new UsageInformation(Constants.Telemetry.RichTextEditorCount, richTextDataTypes.Count());

        foreach (IDataType richTextDataType in richTextDataTypes)
        {
            if (richTextDataType.Configuration is not RichTextConfiguration richTextConfiguration)
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
