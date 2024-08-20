using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

public class ConvertBlockGridEditorProperties : ConvertBlockEditorPropertiesBase
{
    /// <summary>
    /// Setting this property to true will cause the migration to be skipped.
    /// </summary>
    /// <remarks>
    /// If you choose to skip the migration, you're responsible for performing the content migration for Block Grids after the V15 upgrade has completed.
    /// </remarks>
    public static bool SkipMigration { get; set; } = false;

    protected override bool SkipThisMigration() => SkipMigration;

    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.BlockGrid };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is BlockValue blockValue
            ? blockValue.ContentData.Any()
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

    public ConvertBlockGridEditorProperties(
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
}
