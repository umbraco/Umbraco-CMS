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
/// Converts Block List editor properties as part of the upgrade process to version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class ConvertBlockListEditorProperties : ConvertBlockEditorPropertiesBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertBlockListEditorProperties"/> class.
    /// </summary>
    /// <param name="context">The migration context. (<see cref="IMigrationContext"/>)</param>
    /// <param name="logger">The logger instance. (<see cref="ILogger{ConvertBlockEditorPropertiesBase}"/>)</param>
    /// <param name="contentTypeService">The content type service. (<see cref="IContentTypeService"/>)</param>
    /// <param name="dataTypeService">The data type service. (<see cref="IDataTypeService"/>)</param>
    /// <param name="jsonSerializer">The JSON serializer. (<see cref="IJsonSerializer"/>)</param>
    /// <param name="umbracoContextFactory">The Umbraco context factory. (<see cref="IUmbracoContextFactory"/>)</param>
    /// <param name="languageService">The language service. (<see cref="ILanguageService"/>)</param>
    /// <param name="options">The options for converting block editor properties. (<see cref="IOptions{ConvertBlockEditorPropertiesOptions}"/>)</param>
    /// <param name="coreScopeProvider">The core scope provider. (<see cref="ICoreScopeProvider"/>)</param>
    public ConvertBlockListEditorProperties(
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
        SkipMigration = options.Value.SkipBlockListEditors;
        ParallelizeMigration = options.Value.ParallelizeMigration;
    }

    protected override IEnumerable<string> PropertyEditorAliases
        => new[] { Constants.PropertyEditors.Aliases.BlockList };

    protected override EditorValueHandling DetermineEditorValueHandling(object editorValue)
        => editorValue is BlockValue blockValue
            ? blockValue.ContentData.Any()
                ? EditorValueHandling.ProceedConversion
                : EditorValueHandling.IgnoreConversion
            : EditorValueHandling.HandleAsError;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertBlockListEditorProperties"/> class, used for migrating and converting Block List editor properties during the Umbraco upgrade process.
    /// </summary>
    /// <param name="context">The migration context providing information and services for the migration.</param>
    /// <param name="logger">The logger used for logging migration events and information.</param>
    /// <param name="contentTypeService">Service for managing content types.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="jsonSerializer">The serializer used for handling JSON data.</param>
    /// <param name="umbracoContextFactory">Factory for creating Umbraco context instances.</param>
    /// <param name="languageService">Service for managing languages.</param>
    /// <param name="coreScopeProvider">Provider for managing database scopes during migration.</param>
    public ConvertBlockListEditorProperties(
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
