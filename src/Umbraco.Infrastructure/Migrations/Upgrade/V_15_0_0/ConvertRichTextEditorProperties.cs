using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Will be removed in V18")]
public partial class ConvertRichTextEditorProperties : ConvertBlockEditorPropertiesBase
{
    public ConvertRichTextEditorProperties(
        IMigrationContext context,
        ILogger<ConvertBlockEditorPropertiesBase> logger,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IJsonSerializer jsonSerializer,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IOptions<ConvertBlockEditorPropertiesOptions> options,
        ICoreScopeProvider coreScopeProvider)
        : base(context, logger, contentTypeService, dataTypeService, jsonSerializer, umbracoContextFactory, languageService, coreScopeProvider)
    {
        SkipMigration = options.Value.SkipRichTextEditors;
        ParallelizeMigration = options.Value.ParallelizeMigration;
    }

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
        ILanguageService languageService,
        ICoreScopeProvider coreScopeProvider)
        : base(context, logger, contentTypeService, dataTypeService, jsonSerializer, umbracoContextFactory, languageService, coreScopeProvider)
    {
    }

    protected override bool IsCandidateForMigration(IPropertyType propertyType, IDataType dataType)
        => dataType.ConfigurationObject is RichTextConfiguration richTextConfiguration
           && richTextConfiguration.Blocks?.Any() is true;

    [GeneratedRegex("<umb-rte-block.*(?<attribute>data-content-udi)=\"(?<udi>.[^\"]*)\".*<\\/umb-rte-block")]
    private static partial Regex BlockRegex();
}
