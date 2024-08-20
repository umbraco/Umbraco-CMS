using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

public class ConvertRichTextEditorProperties : ConvertBlockEditorPropertiesBase
{
    /// <summary>
    /// Setting this property to true will cause the migration to be skipped.
    /// </summary>
    /// <remarks>
    /// If you choose to skip the migration, you're responsible for performing the content migration for RTEs after the V15 upgrade has completed.
    /// </remarks>
    public static bool SkipMigration { get; set; } = false;

    protected override bool SkipThisMigration() => SkipMigration;

    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.TinyMce, Constants.PropertyEditors.Aliases.RichText };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is RichTextEditorValue richTextEditorValue
            ? richTextEditorValue.Blocks?.ContentData.Any() is true
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

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

    protected override bool IsCandidateForMigration(IPropertyType propertyType, IDataType dataType)
        => dataType.ConfigurationObject is RichTextConfiguration richTextConfiguration
           && richTextConfiguration.Blocks?.Any() is true;
}
