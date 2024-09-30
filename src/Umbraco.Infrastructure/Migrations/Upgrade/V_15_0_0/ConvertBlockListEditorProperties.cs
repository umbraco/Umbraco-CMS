using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Will be removed in V18")]
public class ConvertBlockListEditorProperties : ConvertBlockEditorPropertiesBase
{
    public ConvertBlockListEditorProperties(
        IMigrationContext context,
        ILogger<ConvertBlockEditorPropertiesBase> logger,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IJsonSerializer jsonSerializer,
        IUmbracoContextFactory umbracoContextFactory,
        ILanguageService languageService,
        IOptions<ConvertBlockEditorPropertiesOptions> options)
        : base(context, logger, contentTypeService, dataTypeService, jsonSerializer, umbracoContextFactory, languageService)
        => SkipMigration = options.Value.SkipBlockListEditors;

    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.BlockList };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is BlockValue blockValue
            ? blockValue.ContentData.Any()
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

    public ConvertBlockListEditorProperties(
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
