using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Override the base content repository so we can change the node object type
/// </summary>
/// <remarks>
///     It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class,
///     however to do that
///     requires quite a lot of work since we'd need to re-organize the inheritance quite a lot or create a helper class to
///     perform a lot of the underlying logic.
///     TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
/// </remarks>
internal sealed class DocumentBlueprintRepository : DocumentRepository, IDocumentBlueprintRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBlueprintRepository"/> class, which manages document blueprints in the persistence layer.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="appCaches">The cache manager for application-level and request-level caching.</param>
    /// <param name="logger">The logger used for logging repository operations and errors.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="contentTypeRepository">Repository for accessing content type definitions.</param>
    /// <param name="templateRepository">Repository for accessing template entities.</param>
    /// <param name="tagRepository">Repository for managing tags associated with content.</param>
    /// <param name="languageRepository">Repository for accessing language definitions.</param>
    /// <param name="relationRepository">Repository for managing entity relations.</param>
    /// <param name="relationTypeRepository">Repository for managing relation types.</param>
    /// <param name="propertyEditorCollection">Collection of property editors used for content properties.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="dataValueReferenceFactories">Collection of factories for resolving data value references.</param>
    /// <param name="serializer">The JSON serializer for serializing and deserializing data.</param>
    /// <param name="eventAggregator">Publishes and subscribes to domain events.</param>
    public DocumentBlueprintRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<DocumentBlueprintRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITemplateRepository templateRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            contentTypeRepository,
            templateRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditorCollection,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator)
    {
    }

    protected override bool EnsureUniqueNaming => false; // duplicates are allowed

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprint;
}
