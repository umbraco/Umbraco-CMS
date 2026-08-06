using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides an EF Core backed async repository for <see cref="IContent" /> document blueprint (content template) entities.
/// </summary>
internal sealed class AsyncDocumentBlueprintRepository : AsyncDocumentRepository, IAsyncDocumentBlueprintRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncDocumentBlueprintRepository" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="relationRepository">The relation repository.</param>
    /// <param name="relationTypeRepository">The relation type repository.</param>
    /// <param name="propertyEditors">The property editor collection.</param>
    /// <param name="dataValueReferenceFactories">The data value reference factory collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="eventAggregator">The event aggregator for unit-of-work notifications.</param>
    /// <param name="repositoryCacheVersionService">The repository cache version service.</param>
    /// <param name="cacheSyncService">The cache synchronization service.</param>
    /// <param name="contentTypeRepository">The content type repository.</param>
    /// <param name="templateRepository">The template repository, used to validate template IDs on load.</param>
    /// <param name="idKeyMap">The ID/key map, used to resolve data type configuration for sortable property values.</param>
    /// <param name="tagRepository">The tag repository, used to persist tag values for tag-enabled properties on publish.</param>
    /// <param name="jsonSerializer">The JSON serializer, used to parse legacy JSON-stored tag values.</param>
    /// <param name="userGroupService">The user group service, used to resolve user group keys to IDs for permission storage.</param>
    /// <param name="shortStringHelper">The short string helper, used to detect URL segment collisions between sibling names.</param>
    internal AsyncDocumentBlueprintRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        ILoggerFactory loggerFactory,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService,
        IContentTypeRepository contentTypeRepository,
        ITemplateRepository templateRepository,
        IIdKeyMap idKeyMap,
        ITagRepository tagRepository,
        IJsonSerializer jsonSerializer,
        IUserGroupService userGroupService,
        IShortStringHelper shortStringHelper)
        : base(
            scopeAccessor,
            appCaches,
            loggerFactory,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService,
            contentTypeRepository,
            templateRepository,
            idKeyMap,
            tagRepository,
            jsonSerializer,
            userGroupService,
            shortStringHelper)
    {
    }

    /// <inheritdoc />
    protected override bool EnsureUniqueNaming => false; // duplicates are allowed

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.DocumentBlueprint;
}
