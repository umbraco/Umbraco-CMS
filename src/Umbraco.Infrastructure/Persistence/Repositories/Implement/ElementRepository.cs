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
    private readonly AppCaches _appCaches;
    private readonly IContentTypeRepository _contentTypeRepository;

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
            cacheSyncService)
    {
        _contentTypeRepository =
            contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _appCaches = appCaches;
    }

    protected override ElementRepository This => this;

    /// <summary>
    ///     Default is to always ensure all elements have unique names
    /// </summary>
    protected virtual bool EnsureUniqueNaming { get; } = true;

    protected override IEnumerable<IElement> MapDtosToContent(List<ElementDto> dtos,
        bool withCache = false,
        string[]? propertyAliases = null,
        bool loadTemplates = true,
        bool loadVariants = true)
    {
        var temps = new List<TempContent<IElement>>();
        var contentTypes = new Dictionary<int, IContentType?>();

        var content = new IElement[dtos.Count];

        for (var i = 0; i < dtos.Count; i++)
        {
            ElementDto dto = dtos[i];

            if (withCache)
            {
                // if the cache contains the (proper version of the) item, use it
                IElement? cached =
                    IsolatedCache.GetCacheItem<IElement>(RepositoryCacheKeys.GetKey<IElement, int>(dto.NodeId));
                if (cached != null && cached.VersionId == dto.ContentVersionDto.ContentVersionDto.Id)
                {
                    content[i] = cached;
                    continue;
                }
            }

            // else, need to build it

            // get the content type - the repository is full cache *but* still deep-clones
            // whatever comes out of it, so use our own local index here to avoid this
            var contentTypeId = dto.ContentDto.ContentTypeId;
            if (contentTypes.TryGetValue(contentTypeId, out IContentType? contentType) == false)
            {
                contentTypes[contentTypeId] = contentType = _contentTypeRepository.Get(contentTypeId);
            }

            IElement c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

            // need temps, for properties, templates and variations
            var versionId = dto.ContentVersionDto.Id;
            var publishedVersionId = dto.Published ? dto.PublishedVersionDto!.Id : 0;
            var temp = new TempContent<IElement>(dto.NodeId, versionId, publishedVersionId, contentType, c);

            temps.Add(temp);
        }

        // An empty array of propertyAliases indicates that no properties need to be loaded (null = load all properties).
        var loadProperties = propertyAliases is { Length: 0 } is false;

        IDictionary<int, PropertyCollection>? properties = null;
        if (loadProperties)
        {
            // load all properties for all elements from database in 1 query - indexed by version id
            properties = GetPropertyCollections(temps, propertyAliases);
        }

        // assign templates and properties
        foreach (TempContent<IElement> temp in temps)
        {
            // set properties
            if (loadProperties)
            {
                if (properties?.ContainsKey(temp.VersionId) ?? false)
                {
                    temp.Content!.Properties = properties[temp.VersionId];
                }
                else
                {
                    throw new InvalidOperationException($"No property data found for version: '{temp.VersionId}'.");
                }
            }
            else
            {
                // When loadProperties is false (propertyAliases is empty array), clear the property collection
                temp.Content!.Properties = new PropertyCollection();
            }
        }

        if (loadVariants)
        {
            // set variations, if varying
            temps = temps.Where(x => x.ContentType?.VariesByCulture() ?? false).ToList();
            if (temps.Count > 0)
            {
                // load all variations for all elements from database, in one query
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(temps);
                IDictionary<int, List<EntityVariation>> elementVariations = GetEntityVariations(temps);
                foreach (TempContent<IElement> temp in temps)
                {
                    SetVariations(temp.Content, contentVariations, elementVariations);
                }
            }
        }

        foreach (IElement c in content)
        {
            c.ResetDirtyProperties(false); // reset dirty initial properties (U4-1946)
        }

        return content;
    }

    protected override IElement MapDtoToContent(ElementDto dto)
    {
        IContentType? contentType = _contentTypeRepository.Get(dto.ContentDto.ContentTypeId);
        IElement content = ContentBaseFactory.BuildEntity(dto, contentType);

        try
        {
            content.DisableChangeTracking();

            // get properties - indexed by version id
            var versionId = dto.ContentVersionDto.Id;

            // TODO: shall we get published properties or not?
            //var publishedVersionId = dto.Published ? dto.PublishedVersionDto.Id : 0;
            var publishedVersionId = dto.PublishedVersionDto?.Id ?? 0;

            var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType);
            var ltemp = new List<TempContent<Content>> {temp};
            IDictionary<int, PropertyCollection> properties = GetPropertyCollections(ltemp);
            content.Properties = properties[dto.ContentVersionDto.Id];

            // set variations, if varying
            if (contentType?.VariesByCulture() ?? false)
            {
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(ltemp);
                IDictionary<int, List<EntityVariation>> elementVariations = GetEntityVariations(ltemp);
                SetVariations(content, contentVariations, elementVariations);
            }

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }
        finally
        {
            content.EnableChangeTracking();
        }
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

    // NOTE: Elements cannot have unpublished parents
    public bool IsPathPublished(IElement? content)
        => content is { Trashed: false, Published: true };

    #endregion

    #region Recycle Bin


    public override int RecycleBinId => Constants.System.RecycleBinElement;

    protected override string RecycleBinCacheKey => CacheKeys.ElementRecycleBinCacheKey;

    #endregion
}
