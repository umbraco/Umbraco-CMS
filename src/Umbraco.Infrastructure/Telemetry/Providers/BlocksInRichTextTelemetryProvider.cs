using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data related to blocks used within rich text editors.
/// </summary>
public class BlocksInRichTextTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlocksInRichTextTelemetryProvider"/> class, used for collecting telemetry data about blocks in rich text editors.
    /// </summary>
    /// <param name="dataTypeService">The service used to access and manage data types within the CMS.</param>
    public BlocksInRichTextTelemetryProvider(IDataTypeService dataTypeService)
    {
        _dataTypeService = dataTypeService;
    }

    /// <summary>
    /// Retrieves telemetry usage statistics related to blocks within rich text editors.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="Umbraco.Cms.Infrastructure.Telemetry.UsageInformation"/> objects, where each entry contains statistics such as the total number of rich text editors and the total number of registered blocks within those editors.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IDataType> richTextDataTypes = _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.RichText).GetAwaiter().GetResult().ToArray();
        int registeredBlocks = 0;
        yield return new UsageInformation(Constants.Telemetry.RichTextEditorCount, richTextDataTypes.Count());

        foreach (IDataType richTextDataType in richTextDataTypes)
        {
            if (richTextDataType.ConfigurationObject is not RichTextConfiguration richTextConfiguration)
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
