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

/// <summary>
/// Migration responsible for converting rich text editor properties to the new format as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public partial class ConvertRichTextEditorProperties : ConvertBlockEditorPropertiesBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertRichTextEditorProperties"/> class.
    /// </summary>
    /// <param name="context">The migration context for the operation.</param>
    /// <param name="logger">The logger instance for <see cref="ConvertBlockEditorPropertiesBase"/> operations.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="jsonSerializer">The serializer used for JSON operations.</param>
    /// <param name="umbracoContextFactory">Factory for creating Umbraco context instances.</param>
    /// <param name="languageService">Service for managing languages.</param>
    /// <param name="options">Options for converting block editor properties.</param>
    /// <param name="coreScopeProvider">Provider for managing core scopes.</param>
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
        => new[] { "Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText };

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertRichTextEditorProperties"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="logger">The <see cref="ILogger{ConvertBlockEditorPropertiesBase}"/> used for logging migration events.</param>
    /// <param name="contentTypeService">The <see cref="IContentTypeService"/> used to manage content types.</param>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> used to manage data types.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="umbracoContextFactory">The <see cref="IUmbracoContextFactory"/> used to create Umbraco contexts.</param>
    /// <param name="languageService">The <see cref="ILanguageService"/> used to manage languages.</param>
    /// <param name="coreScopeProvider">The <see cref="ICoreScopeProvider"/> used to manage database scopes.</param>
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
