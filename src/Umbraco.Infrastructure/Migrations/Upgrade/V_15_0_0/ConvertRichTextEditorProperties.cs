using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

// TODO KJA: only convert RTEs with blocks? Depends on UDI conversion or not.
public partial class ConvertRichTextEditorProperties : ConvertBlockEditorPropertiesBase
{
    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.TinyMce, Constants.PropertyEditors.Aliases.RichText };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is RichTextEditorValue richTextEditorValue
            ? richTextEditorValue.Blocks?.ContentData.Any() is true
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

    protected override object UpdateEditorValue(object editorValue)
    {
        if (editorValue is not RichTextEditorValue richTextEditorValue)
        {
            return base.UpdateEditorValue(editorValue);
        }

        richTextEditorValue.Markup = BlockRegex().Replace(
            richTextEditorValue.Markup,
            match => UdiParser.TryParse(match.Groups["udi"].Value, out GuidUdi? guidUdi)
                ? match.Value
                    .Replace(match.Groups["attribute"].Value, "data-content-key")
                    .Replace(match.Groups["udi"].Value, guidUdi.Guid.ToString("D"))
                : string.Empty);

        return richTextEditorValue;
    }

    public ConvertRichTextEditorProperties(
        IMigrationContext context,
        ILogger<ConvertBlockEditorPropertiesBase> logger,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IJsonSerializer jsonSerializer,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService)
        : base(context, logger, contentTypeService, dataTypeService, jsonSerializer, umbracoContextFactory, languageService)
    {
    }

    [GeneratedRegex("<umb-rte-block.*(?<attribute>data-content-udi)=\"(?<udi>.[^\"]*)\".*<\\/umb-rte-block")]
    private static partial Regex BlockRegex();
}
