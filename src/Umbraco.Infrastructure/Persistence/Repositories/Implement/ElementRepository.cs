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
    ///     Constructor
    /// </summary>
    /// <param name="scopeAccessor"></param>
    /// <param name="appCaches"></param>
    /// <param name="logger"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="contentTypeRepository"></param>
    /// <param name="tagRepository"></param>
    /// <param name="languageRepository"></param>
    /// <param name="relationRepository"></param>
    /// <param name="relationTypeRepository"></param>
    /// <param name="dataValueReferenceFactories"></param>
    /// <param name="dataTypeService"></param>
    /// <param name="serializer"></param>
    /// <param name="eventAggregator"></param>
    /// <param name="propertyEditors">
    ///     Lazy property value collection - must be lazy because we have a circular dependency since some property editors
    ///     require services, yet these services require property editors
    /// </param>
    /// <param name="repositoryCacheVersionService"></param>
    /// <param name="cacheSyncService"></param>
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

    // NOTE: Elements cannot have unpublished parents
    public bool IsPathPublished(IElement? content)
        => content is { Trashed: false, Published: true };

    #endregion

    #region Recycle Bin


    public override int RecycleBinId => Constants.System.RecycleBinElement;

    protected override string RecycleBinCacheKey => CacheKeys.ElementRecycleBinCacheKey;

    #endregion
}
