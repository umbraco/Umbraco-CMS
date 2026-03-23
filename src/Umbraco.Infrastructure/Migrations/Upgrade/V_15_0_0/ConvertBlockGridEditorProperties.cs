using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Migration step that converts Block Grid editor properties during the upgrade to Umbraco 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class ConvertBlockGridEditorProperties : ConvertBlockEditorPropertiesBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertBlockGridEditorProperties"/> class.
    /// </summary>
    /// <param name="context">The migration context for the upgrade process.</param>
    /// <param name="logger">The logger for logging migration events and information.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="jsonSerializer">Serializer for handling JSON data.</param>
    /// <param name="umbracoContextFactory">Factory for creating Umbraco context instances.</param>
    /// <param name="languageService">Service for managing languages.</param>
    /// <param name="options">Configuration options for converting block grid editor properties.</param>
    /// <param name="coreScopeProvider">Provider for managing database scopes during migration.</param>
    public ConvertBlockGridEditorProperties(
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
        SkipMigration = options.Value.SkipBlockGridEditors;
        ParallelizeMigration = options.Value.ParallelizeMigration;
    }

    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.BlockGrid };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is BlockValue blockValue
            ? blockValue.ContentData.Any()
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertBlockGridEditorProperties"/> class for migrating Block Grid editor properties.
    /// </summary>
    /// <param name="context">The migration context providing information and services for the migration process.</param>
    /// <param name="logger">The logger used to record migration events and errors.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="jsonSerializer">The serializer used for handling JSON data.</param>
    /// <param name="umbracoContextFactory">Factory for creating Umbraco context instances.</param>
    /// <param name="languageService">Service for managing languages and translations.</param>
    /// <param name="coreScopeProvider">Provider for managing database transaction scopes.</param>
    public ConvertBlockGridEditorProperties(
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
}
