using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IElement" />.
/// </summary>
// TODO ELEMENTS: refactor and reuse code from DocumentRepository (note there is an NPoco issue with generics, so we have to live with a certain amount of code duplication)
internal class ElementRepository : PublishableContentRepositoryBase<IElement, ElementRepository, ElementDto, ElementVersionDto, ElementCultureVariationDto>, IElementRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="appCaches">The application-level cache helpers for performance optimization.</param>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="contentTypeRepository">Repository for accessing content type definitions.</param>
    /// <param name="tagRepository">Repository for managing tags associated with elements.</param>
    /// <param name="languageRepository">Repository for managing language entities.</param>
    /// <param name="relationRepository">Repository for managing entity relations.</param>
    /// <param name="relationTypeRepository">Repository for managing relation types.</param>
    /// <param name="propertyEditors">Collection of property editors used for element properties.</param>
    /// <param name="dataValueReferenceFactories">Collection of factories for resolving data value references.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="serializer">JSON serializer for serializing and deserializing data.</param>
    /// <param name="eventAggregator">Publishes and subscribes to domain events.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public ElementRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<ElementRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService,
            contentTypeRepository)
    {
    }

    protected override ElementRepository This => this;

    protected override ElementDto BuildEntityDto(IElement entity)
        => ContentBaseFactory.BuildDto(entity, NodeObjectTypeId);

    protected override IElement BuildEntity(ElementDto entityDto, IContentType? contentType)
        => ContentBaseFactory.BuildEntity(entityDto, contentType);

    protected override void OnUowRefreshedEntity(IElement entity)
    {
        // TODO ELEMENTS: implement this for elements
    }

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Element;

    protected override IEnumerable<string> GetEntityDeleteClauses()
    {
        var nodeId = QuoteColumnName("nodeId");
        return [
            $@"UPDATE {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup)}
              SET {QuoteColumnName("startElementId")} = NULL
              WHERE {QuoteColumnName("startElementId")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Element)} WHERE {nodeId} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ElementCultureVariation)} WHERE {nodeId} = @id",
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ElementVersion)} WHERE id IN
              (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
        ];
    }

    #endregion

    #region Content Repository

    /// <summary>
    /// Determines whether the specified element and all its ancestors in the content path are published.
    /// Elements cannot have unpublished parents, so this simply checks the element itself.
    /// </summary>
    /// <param name="content">The element to check.</param>
    /// <returns><c>true</c> if the element is not trashed and is published; otherwise, <c>false</c>.</returns>
    public bool IsPathPublished(IElement? content)
        => content is { Trashed: false, Published: true };

    #endregion

    #region Recycle Bin

    /// <summary>
    /// Gets the identifier for the element Recycle Bin in Umbraco.
    /// </summary>
    public override int RecycleBinId => Constants.System.RecycleBinElement;

    /// <summary>
    /// Gets the cache key for the element Recycle Bin.
    /// </summary>
    protected override string RecycleBinCacheKey => CacheKeys.ElementRecycleBinCacheKey;

    #endregion
}
