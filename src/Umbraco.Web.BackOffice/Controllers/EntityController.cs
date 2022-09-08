using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.BackOffice.ModelBinders;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are
///     based on CMSNode
/// </summary>
/// <remarks>
///     <para>
///         This controller allows resolving basic entity data for various entities without placing the hard restrictions
///         on users that may not have access
///         to the sections these entities entities exist in. This is to allow pickers, etc... of data to work for all
///         users. In some cases such as accessing
///         Members, more explicit security checks are done.
///     </para>
///     <para>Some objects such as macros are not based on CMSNode</para>
/// </remarks>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[ParameterSwapControllerActionSelector(nameof(GetAncestors), "id", typeof(int), typeof(Guid))]
[ParameterSwapControllerActionSelector(nameof(GetPagedChildren), "id", typeof(int), typeof(string))]
[ParameterSwapControllerActionSelector(nameof(GetPath), "id", typeof(int), typeof(Guid), typeof(Udi))]
[ParameterSwapControllerActionSelector(nameof(GetUrlAndAnchors), "id", typeof(int), typeof(Guid), typeof(Udi))]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
[ParameterSwapControllerActionSelector(nameof(GetByIds), "ids", typeof(int[]), typeof(Guid[]), typeof(Udi[]))]
[ParameterSwapControllerActionSelector(nameof(GetUrl), "id", typeof(int), typeof(Udi))]
[ParameterSwapControllerActionSelector(nameof(GetUrlsByIds), "ids", typeof(int[]), typeof(Guid[]), typeof(Udi[]))]
public class EntityController : UmbracoAuthorizedJsonController
{
    private static readonly string[] _postFilterSplitStrings = { "=", "==", "!=", "<>", ">", "<", ">=", "<=" };

    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IEntityService _entityService;
    private readonly IFileService _fileService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMacroService _macroService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly SearchableTreeCollection _searchableTreeCollection;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ISqlContext _sqlContext;
    private readonly UmbracoTreeSearcher _treeSearcher;
    private readonly ITreeService _treeService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IUserService _userService;

    public EntityController(
        ITreeService treeService,
        UmbracoTreeSearcher treeSearcher,
        SearchableTreeCollection searchableTreeCollection,
        IPublishedContentQuery publishedContentQuery,
        IShortStringHelper shortStringHelper,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IPublishedUrlProvider publishedUrlProvider,
        IContentService contentService,
        IUmbracoMapper umbracoMapper,
        IDataTypeService dataTypeService,
        ISqlContext sqlContext,
        ILocalizedTextService localizedTextService,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMacroService macroService,
        IUserService userService,
        ILocalizationService localizationService,
        AppCaches appCaches)
    {
        _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
        _treeSearcher = treeSearcher ?? throw new ArgumentNullException(nameof(treeSearcher));
        _searchableTreeCollection = searchableTreeCollection ??
                                    throw new ArgumentNullException(nameof(searchableTreeCollection));
        _publishedContentQuery =
            publishedContentQuery ?? throw new ArgumentNullException(nameof(publishedContentQuery));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _publishedUrlProvider =
            publishedUrlProvider ?? throw new ArgumentNullException(nameof(publishedUrlProvider));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
        _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        _sqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
        _localizedTextService =
            localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
        _mediaTypeService = mediaTypeService ?? throw new ArgumentNullException(nameof(mediaTypeService));
        _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _appCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
    }


    /// <summary>
    ///     Returns an Umbraco alias given a string
    /// </summary>
    /// <param name="value"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    public dynamic GetSafeAlias(string value, bool camelCase = true)
    {
        var returnValue = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.ToSafeAlias(_shortStringHelper, camelCase);
        dynamic returnObj = new ExpandoObject();
        returnObj.alias = returnValue;
        returnObj.original = value;
        returnObj.camelCase = camelCase;

        return returnObj;
    }

    /// <summary>
    ///     Searches for results based on the entity type
    /// </summary>
    /// <param name="query"></param>
    /// <param name="type"></param>
    /// <param name="searchFrom">
    ///     A starting point for the search, generally a node id, but for members this is a member type alias
    /// </param>
    /// <param name="dataTypeKey">If set used to look up whether user and group start node permissions will be ignored.</param>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes type, string? searchFrom = null, Guid? dataTypeKey = null)
    {
        // NOTE: Theoretically you shouldn't be able to see member data if you don't have access to members right? ... but there is a member picker, so can't really do that

        if (string.IsNullOrEmpty(query))
        {
            return Enumerable.Empty<EntityBasic>();
        }

        //TODO: This uses the internal UmbracoTreeSearcher, this instead should delgate to the ISearchableTree implementation for the type

        var ignoreUserStartNodes = dataTypeKey.HasValue &&
                                   _dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeKey.Value);
        return ExamineSearch(query, type, searchFrom, ignoreUserStartNodes);
    }

        /// <summary>
        ///     Searches for all content that the user is allowed to see (based on their allowed sections)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Even though a normal entity search will allow any user to search on a entity type that they may not have access to
        ///     edit, we need
        ///     to filter these results to the sections they are allowed to edit since this search function is explicitly for the
        ///     global search
        ///     so if we showed entities that they weren't allowed to edit they would get errors when clicking on the result.
        ///     The reason a user is allowed to search individual entity types that they are not allowed to edit is because those
        ///     search
        ///     methods might be used in things like pickers in the content editor.
        /// </remarks>
        [HttpGet]
        public async Task<IDictionary<string, TreeSearchResult>> SearchAll(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new Dictionary<string, TreeSearchResult>();
            }

            var culture = ClientCulture();
            var allowedSections = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.AllowedSections.ToArray();

            var searchTasks = new List<Task<KeyValuePair<string, TreeSearchResult>>>();
            foreach (KeyValuePair<string, SearchableApplicationTree> searchableTree in _searchableTreeCollection
                         .SearchableApplicationTrees.OrderBy(t => t.Value.SortOrder))
            {
                if (allowedSections?.Contains(searchableTree.Value.AppAlias) ?? false)
                {
                    Tree? tree = _treeService.GetByAlias(searchableTree.Key);
                    if (tree == null)
                    {
                        continue; //shouldn't occur
                    }

                    var rootNodeDisplayName = Tree.GetRootNodeDisplayName(tree, _localizedTextService);
                    if (rootNodeDisplayName is not null)
                    {
                        searchTasks.Add(ExecuteSearchAsync(query, culture, searchableTree, rootNodeDisplayName));
                    }
                }
            }

            var taskResults = await Task.WhenAll(searchTasks);

            return new Dictionary<string, TreeSearchResult>(taskResults);
        }

        private static async Task<KeyValuePair<string, TreeSearchResult>> ExecuteSearchAsync(
            string query,
            string? culture,
            KeyValuePair<string, SearchableApplicationTree> searchableTree,
            string rootNodeDisplayName)
        {
            ISearchableTree searcher = searchableTree.Value.SearchableTree;
            const int pageSize = 200;
            IEnumerable<SearchResultEntity> results = (
                searcher is ISearchableTreeWithCulture searcherWithCulture
                    ? await searcherWithCulture.SearchAsync(query, pageSize, 0, culture: culture)
                    : await searcher.SearchAsync(query, pageSize, 0))
            .WhereNotNull();

            var searchResult = new TreeSearchResult
            {
                Results = results,
                TreeAlias = searchableTree.Key,
                AppAlias = searchableTree.Value.AppAlias,
                JsFormatterService = searchableTree.Value.FormatterService,
                JsFormatterMethod = searchableTree.Value.FormatterMethod
            };

            return new KeyValuePair<string, TreeSearchResult>(rootNodeDisplayName, searchResult);
        }

        /// <summary>
        ///     Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IConvertToActionResult GetPath(int id, UmbracoEntityTypes type)
        {
            ActionResult<EntityBasic?> foundContentResult = GetResultForId(id, type);
            EntityBasic? foundContent = foundContentResult.Value;
            if (foundContent is null)
            {
                return foundContentResult;
            }

        return new ActionResult<IEnumerable<int>>(foundContent.Path
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Select(
                s => int.Parse(s, CultureInfo.InvariantCulture)));
    }

    /// <summary>
    ///     Gets the path for a given node ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IConvertToActionResult GetPath(Guid id, UmbracoEntityTypes type)
    {
        ActionResult<EntityBasic?> foundContentResult = GetResultForKey(id, type);
        EntityBasic? foundContent = foundContentResult.Value;
        if (foundContent is null)
        {
            return foundContentResult;
        }

        return new ActionResult<IEnumerable<int>>(foundContent.Path
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).Select(
                s => int.Parse(s, CultureInfo.InvariantCulture)));
    }

    /// <summary>
    ///     Gets the path for a given node ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public IActionResult GetPath(Udi id, UmbracoEntityTypes type)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi != null)
        {
            return GetPath(guidUdi.Guid, type).Convert();
        }

        return NotFound();
    }

    /// <summary>
    ///     Gets the URL of an entity
    /// </summary>
    /// <param name="id">UDI of the entity to fetch URL for</param>
    /// <param name="culture">The culture to fetch the URL for</param>
    /// <returns>The URL or path to the item</returns>
    public IActionResult GetUrl(Udi id, string culture = "*")
    {
        Attempt<int> intId = _entityService.GetId(id);
        if (!intId.Success)
        {
            return NotFound();
        }

        UmbracoEntityTypes entityType;
        switch (id.EntityType)
        {
            case Constants.UdiEntityType.Document:
                entityType = UmbracoEntityTypes.Document;
                break;
            case Constants.UdiEntityType.Media:
                entityType = UmbracoEntityTypes.Media;
                break;
            case Constants.UdiEntityType.Member:
                entityType = UmbracoEntityTypes.Member;
                break;
            default:
                return NotFound();
        }

        return GetUrl(intId.Result, entityType, culture);
    }

    /// <summary>
    ///     Get entity URLs by IDs
    /// </summary>
    /// <param name="ids">
    ///     A list of IDs to lookup items by
    /// </param>
    /// <param name="type">The entity type to look for.</param>
    /// <param name="culture">The culture to fetch the URL for.</param>
    /// <returns>Dictionary mapping Udi -> Url</returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids.
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public IDictionary<int, string?> GetUrlsByIds([FromJsonPath] int[] ids, [FromQuery] UmbracoEntityTypes type, [FromQuery] string? culture = null)
    {
        if (ids == null || !ids.Any())
        {
            return new Dictionary<int, string?>();
        }

        string? MediaOrDocumentUrl(int id)
        {
            switch (type)
            {
                case UmbracoEntityTypes.Document:
                    return _publishedUrlProvider.GetUrl(id, culture: culture ?? ClientCulture());

                case UmbracoEntityTypes.Media:
                {
                    IPublishedContent? media = _publishedContentQuery.Media(id);

                    // NOTE: If culture is passed here we get an empty string rather than a media item URL.
                    return _publishedUrlProvider.GetMediaUrl(media, culture: null);
                }

                default:
                    return null;
            }
        }

        return ids
            .Distinct()
            .Select(id => new { Id = id, Url = MediaOrDocumentUrl(id) }).ToDictionary(x => x.Id, x => x.Url);
    }

    /// <summary>
    ///     Get entity URLs by IDs
    /// </summary>
    /// <param name="ids">
    ///     A list of IDs to lookup items by
    /// </param>
    /// <param name="type">The entity type to look for.</param>
    /// <param name="culture">The culture to fetch the URL for.</param>
    /// <returns>Dictionary mapping Udi -> Url</returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids.
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public IDictionary<Guid, string?> GetUrlsByIds([FromJsonPath] Guid[] ids, [FromQuery] UmbracoEntityTypes type, [FromQuery] string? culture = null)
    {
        if (ids == null || !ids.Any())
        {
            return new Dictionary<Guid, string?>();
        }

        string? MediaOrDocumentUrl(Guid id)
        {
            return type switch
            {
                UmbracoEntityTypes.Document => _publishedUrlProvider.GetUrl(id, culture: culture ?? ClientCulture()),

                // NOTE: If culture is passed here we get an empty string rather than a media item URL.
                UmbracoEntityTypes.Media => _publishedUrlProvider.GetMediaUrl(id, culture: null),

                _ => null
            };
        }

        return ids
            .Distinct()
            .Select(id => new { Id = id, Url = MediaOrDocumentUrl(id) }).ToDictionary(x => x.Id, x => x.Url);
    }

    /// <summary>
    ///     Get entity URLs by IDs
    /// </summary>
    /// <param name="ids">
    ///     A list of IDs to lookup items by
    /// </param>
    /// <param name="type">The entity type to look for.</param>
    /// <param name="culture">The culture to fetch the URL for.</param>
    /// <returns>Dictionary mapping Udi -> Url</returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids.
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public IDictionary<Udi, string?> GetUrlsByIds([FromJsonPath] Udi[] ids, [FromQuery] UmbracoEntityTypes type, [FromQuery] string? culture = null)
    {
        if (ids == null || !ids.Any())
        {
            return new Dictionary<Udi, string?>();
        }

        // TODO: PMJ 2021-09-27 - Should GetUrl(Udi) exist as an extension method on UrlProvider/IUrlProvider (in v9)
        string? MediaOrDocumentUrl(Udi id)
        {
            if (id is not GuidUdi guidUdi)
            {
                return null;
            }

            return type switch
            {
                UmbracoEntityTypes.Document => _publishedUrlProvider.GetUrl(guidUdi.Guid, culture: culture ?? ClientCulture()),

                // NOTE: If culture is passed here we get an empty string rather than a media item URL.
                UmbracoEntityTypes.Media => _publishedUrlProvider.GetMediaUrl(guidUdi.Guid, culture: null),

                _ => null
            };
        }

        return ids
            .Distinct()
            .Select(id => new { Id = id, Url = MediaOrDocumentUrl(id) }).ToDictionary(x => x.Id, x => x.Url);
    }

    /// <summary>
    ///     Get entity URLs by UDIs
    /// </summary>
    /// <param name="udis">
    ///     A list of UDIs to lookup items by
    /// </param>
    /// <param name="culture">The culture to fetch the URL for</param>
    /// <returns>Dictionary mapping Udi -> Url</returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids.
    /// </remarks>
    [HttpGet]
    [HttpPost]
    [Obsolete("Use GetUrlsByIds instead.")]
    public IDictionary<Udi, string?> GetUrlsByUdis([FromJsonPath] Udi[] udis, string? culture = null)
    {
        if (udis == null || !udis.Any())
        {
            return new Dictionary<Udi, string?>();
        }

        var udiEntityType = udis.First().EntityType;
        UmbracoEntityTypes entityType;

        switch (udiEntityType)
        {
            case Constants.UdiEntityType.Document:
                entityType = UmbracoEntityTypes.Document;
                break;
            case Constants.UdiEntityType.Media:
                entityType = UmbracoEntityTypes.Media;
                break;
            default:
                entityType = (UmbracoEntityTypes)(-1);
                break;
        }

        return GetUrlsByIds(udis, entityType, culture);
    }

    /// <summary>
    ///     Gets the URL of an entity
    /// </summary>
    /// <param name="id">Int id of the entity to fetch URL for</param>
    /// <param name="type">The type of entity such as Document, Media, Member</param>
    /// <param name="culture">The culture to fetch the URL for</param>
    /// <returns>The URL or path to the item</returns>
    /// <remarks>
    ///     We are not restricting this with security because there is no sensitive data
    /// </remarks>
    public IActionResult GetUrl(int id, UmbracoEntityTypes type, string? culture = null)
    {
        culture ??= ClientCulture();

        var returnUrl = string.Empty;

        if (type == UmbracoEntityTypes.Document)
        {
            var foundUrl = _publishedUrlProvider.GetUrl(id, culture: culture);
            if (string.IsNullOrEmpty(foundUrl) == false && foundUrl != "#")
            {
                returnUrl = foundUrl;

                return Ok(returnUrl);
            }
        }

        IEnumerable<EntityBasic> ancestors = GetResultForAncestors(id, type);

        //if content, skip the first node for replicating NiceUrl defaults
        if (type == UmbracoEntityTypes.Document)
        {
            ancestors = ancestors.Skip(1);
        }

        returnUrl = "/" + string.Join("/", ancestors.Select(x => x.Name));

        return Ok(returnUrl);
    }


    /// <summary>
    ///     Gets an entity by a xpath query
    /// </summary>
    /// <param name="query"></param>
    /// <param name="nodeContextId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult<EntityBasic?>? GetByQuery(string query, int nodeContextId, UmbracoEntityTypes type)
    {
        // TODO: Rename this!!! It's misleading, it should be GetByXPath


        if (type != UmbracoEntityTypes.Document)
        {
            throw new ArgumentException("Get by query is only compatible with entities of type Document");
        }


        var q = ParseXPathQuery(query, nodeContextId);
        IPublishedContent? node = _publishedContentQuery.ContentSingleAtXPath(q);

        if (node == null)
        {
            return null;
        }

        return GetById(node.Id, type);
    }

    // PP: Work in progress on the query parser
    private string ParseXPathQuery(string query, int id) =>
        UmbracoXPathPathSyntaxParser.ParseXPathQuery(
            query,
            id,
            nodeid =>
            {
                IEntitySlim? ent = _entityService.Get(nodeid);
                return ent?.Path.Split(Constants.CharArrays.Comma).Reverse();
            },
            i => _publishedContentQuery.Content(i) != null);

    [HttpGet]
    public ActionResult<UrlAndAnchors> GetUrlAndAnchors(Udi id, string culture = "*")
    {
        Attempt<int> intId = _entityService.GetId(id);
        if (!intId.Success)
        {
            return NotFound();
        }

        return GetUrlAndAnchors(intId.Result, culture);
    }

    [HttpGet]
    public UrlAndAnchors GetUrlAndAnchors(int id, string? culture = "*")
    {
        culture ??= ClientCulture();

        var url = _publishedUrlProvider.GetUrl(id, culture: culture);
        IEnumerable<string> anchorValues = _contentService.GetAnchorValuesFromRTEs(id, culture);
        return new UrlAndAnchors(url, anchorValues);
    }

    [HttpGet]
    [HttpPost]
    public IEnumerable<string> GetAnchors(AnchorsModel model)
    {
        if (model.RteContent is null)
        {
            return Enumerable.Empty<string>();
        }

        IEnumerable<string> anchorValues = _contentService.GetAnchorValuesFromRTEContent(model.RteContent);
        return anchorValues;
    }

    public IEnumerable<EntityBasic> GetChildren(int id, UmbracoEntityTypes type, Guid? dataTypeKey = null)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(type);
        if (objectType.HasValue)
        {
            //TODO: Need to check for Object types that support hierarchy here, some might not.

            var startNodes = GetStartNodes(type);

            var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);

            // root is special: we reduce it to start nodes if the user's start node is not the default, then we need to return their start nodes
            if (id == Constants.System.Root && startNodes.Length > 0 &&
                startNodes.Contains(Constants.System.Root) == false && !ignoreUserStartNodes)
            {
                IEntitySlim[] nodes = _entityService.GetAll(objectType.Value, startNodes).ToArray();
                if (nodes.Length == 0)
                {
                    return Enumerable.Empty<EntityBasic>();
                }

                var pr = new List<EntityBasic>(nodes.Select(_umbracoMapper.Map<EntityBasic>).WhereNotNull());
                return pr;
            }

            // else proceed as usual

            return _entityService.GetChildren(id, objectType.Value)
                .Select(_umbracoMapper.Map<EntityBasic>)
                .WhereNotNull();
        }

        //now we need to convert the unknown ones
        switch (type)
        {
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + type);
        }
    }

    /// <summary>
    ///     Get paged child entities by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="filter"></param>
    /// <param name="dataTypeKey"></param>
    /// <returns></returns>
    public ActionResult<PagedResult<EntityBasic>> GetPagedChildren(
        string id,
        UmbracoEntityTypes type,
        int pageNumber,
        int pageSize,
        string orderBy = "SortOrder",
        Direction orderDirection = Direction.Ascending,
        string filter = "",
        Guid? dataTypeKey = null)
    {
        if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
        {
            return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
        }

        if (Guid.TryParse(id, out _))
        {
            //Not supported currently
            return NotFound();
        }

        if (UdiParser.TryParse(id, out _))
        {
            //Not supported currently
            return NotFound();
        }

        //so we don't have an INT, GUID or UDI, it's just a string, so now need to check if it's a special id or a member type
        if (id == Constants.Conventions.MemberTypes.AllMembersListId)
        {
            //the EntityService can search paged members from the root

            intId = -1;
            return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter, dataTypeKey);
        }

        //the EntityService cannot search members of a certain type, this is currently not supported and would require
        //quite a bit of plumbing to do in the Services/Repository, we'll revert to a paged search

        //TODO: We should really fix this in the EntityService but if we don't we should allow the ISearchableTree for the members controller
        // to be used for this search instead of the built in/internal searcher

        IEnumerable<SearchResultEntity> searchResult = _treeSearcher.ExamineSearch(
            filter ?? string.Empty,
            type,
            pageSize,
            pageNumber - 1,
            out var total,
            null,
            id);

        return new PagedResult<EntityBasic>(total, pageNumber, pageSize) { Items = searchResult };
    }

    /// <summary>
    ///     Get paged child entities by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="filter"></param>
    /// <param name="dataTypeKey"></param>
    /// <returns></returns>
    public ActionResult<PagedResult<EntityBasic>> GetPagedChildren(
        int id,
        UmbracoEntityTypes type,
        int pageNumber,
        int pageSize,
        string orderBy = "SortOrder",
        Direction orderDirection = Direction.Ascending,
        string filter = "",
        Guid? dataTypeKey = null)
    {
        if (pageNumber <= 0)
        {
            return NotFound();
        }

        if (pageSize <= 0)
        {
            return NotFound();
        }

        UmbracoObjectTypes? objectType = ConvertToObjectType(type);
        if (objectType.HasValue)
        {
            IEnumerable<IEntitySlim> entities;

            var startNodes = GetStartNodes(type);

            var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);

            // root is special: we reduce it to start nodes if the user's start node is not the default, then we need to return their start nodes
            if (id == Constants.System.Root && startNodes.Length > 0 &&
                startNodes.Contains(Constants.System.Root) == false && !ignoreUserStartNodes)
            {
                return new PagedResult<EntityBasic>(0, 0, 0);
            }

            // else proceed as usual
            entities = _entityService.GetPagedChildren(
                id,
                objectType.Value,
                pageNumber - 1,
                pageSize,
                out long totalRecords,
                filter.IsNullOrWhiteSpace()
                    ? null
                    : _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name!.Contains(filter)),
                Ordering.By(orderBy, orderDirection));


            if (totalRecords == 0)
            {
                return new PagedResult<EntityBasic>(0, 0, 0);
            }

            var culture = ClientCulture();
            var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = entities.Select(source =>
                {
                    EntityBasic? target = _umbracoMapper.Map<IEntitySlim, EntityBasic>(source, context =>
                    {
                        context.SetCulture(culture);
                        context.SetCulture(culture);
                    });

                    if (target is not null)
                    {
                        //TODO: Why is this here and not in the mapping?
                        target.AdditionalData["hasChildren"] = source.HasChildren;
                    }

                    return target;
                }).WhereNotNull()
            };

            return pagedResult;
        }

        //now we need to convert the unknown ones
        switch (type)
        {
            case UmbracoEntityTypes.PropertyType:
            case UmbracoEntityTypes.PropertyGroup:
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + type);
        }
    }

    private int[] GetStartNodes(UmbracoEntityTypes type)
    {
        switch (type)
        {
            case UmbracoEntityTypes.Document:
                return _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.CalculateContentStartNodeIds(
                    _entityService, _appCaches) ?? Array.Empty<int>();
            case UmbracoEntityTypes.Media:
                return _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.CalculateMediaStartNodeIds(
                    _entityService, _appCaches) ?? Array.Empty<int>();
            default:
                return Array.Empty<int>();
        }
    }

    public ActionResult<PagedResult<EntityBasic>> GetPagedDescendants(
        int id,
        UmbracoEntityTypes type,
        int pageNumber,
        int pageSize,
        string orderBy = "SortOrder",
        Direction orderDirection = Direction.Ascending,
        string filter = "",
        Guid? dataTypeKey = null)
    {
        if (pageNumber <= 0)
        {
            return NotFound();
        }

        if (pageSize <= 0)
        {
            return NotFound();
        }

        // re-normalize since NULL can be passed in
        filter ??= string.Empty;

        UmbracoObjectTypes? objectType = ConvertToObjectType(type);
        if (objectType.HasValue)
        {
            IEnumerable<IUmbracoEntity> entities;
            long totalRecords;

            if (id == Constants.System.Root)
            {
                // root is special: we reduce it to start nodes

                var aids = GetStartNodes(type);

                var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);
                entities = aids == null || aids.Contains(Constants.System.Root) || ignoreUserStartNodes
                    ? _entityService.GetPagedDescendants(
                        objectType.Value,
                        pageNumber - 1,
                        pageSize,
                        out totalRecords,
                        _sqlContext.Query<IUmbracoEntity>()
                            .Where(x => x.Name!.Contains(filter)),
                        Ordering.By(orderBy, orderDirection),
                        false)
                    : _entityService.GetPagedDescendants(
                        aids,
                        objectType.Value,
                        pageNumber - 1,
                        pageSize,
                        out totalRecords,
                        _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name!.Contains(filter)),
                        Ordering.By(orderBy, orderDirection));
            }
            else
            {
                entities = _entityService.GetPagedDescendants(
                    id,
                    objectType.Value,
                    pageNumber - 1,
                    pageSize,
                    out totalRecords,
                    _sqlContext.Query<IUmbracoEntity>()
                        .Where(x => x.Name!.Contains(filter)),
                    Ordering.By(orderBy, orderDirection));
            }

            if (totalRecords == 0)
            {
                return new PagedResult<EntityBasic>(0, 0, 0);
            }

            var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = entities.Select(MapEntities()).WhereNotNull()
            };

            return pagedResult;
        }

        //now we need to convert the unknown ones
        switch (type)
        {
            case UmbracoEntityTypes.PropertyType:
            case UmbracoEntityTypes.PropertyGroup:
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + type);
        }
    }

    private bool IsDataTypeIgnoringUserStartNodes(Guid? dataTypeKey) => dataTypeKey.HasValue &&
                                                                        _dataTypeService
                                                                            .IsDataTypeIgnoringUserStartNodes(
                                                                                dataTypeKey.Value);

    public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) =>
        GetResultForAncestors(id, type, queryStrings);

    public ActionResult<IEnumerable<EntityBasic>> GetAncestors(Guid id, UmbracoEntityTypes type, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
    {
        IEntitySlim? entity = _entityService.Get(id);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(GetResultForAncestors(entity.Id, type, queryStrings));
    }

    /// <summary>
    ///     Searches for results based on the entity type
    /// </summary>
    /// <param name="query"></param>
    /// <param name="entityType"></param>
    /// <param name="searchFrom"></param>
    /// <param name="ignoreUserStartNodes">If set to true, user and group start node permissions will be ignored.</param>
    /// <returns></returns>
    private IEnumerable<SearchResultEntity> ExamineSearch(string query, UmbracoEntityTypes entityType, string? searchFrom = null, bool ignoreUserStartNodes = false)
    {
        var culture = ClientCulture();
        return _treeSearcher.ExamineSearch(query, entityType, 200, 0, out _, culture, searchFrom, ignoreUserStartNodes);
    }

    private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            // TODO: Need to check for Object types that support hierarchic here, some might not.

            return _entityService.GetChildren(id, objectType.Value)
                .Select(MapEntities())
                .WhereNotNull();
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private IEnumerable<EntityBasic> GetResultForAncestors(int id, UmbracoEntityTypes entityType, FormCollection? queryStrings = null)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            // TODO: Need to check for Object types that support hierarchic here, some might not.

            var ids = _entityService.Get(id)?.Path.Split(Constants.CharArrays.Comma)
                .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).Distinct().ToArray();

            var ignoreUserStartNodes =
                IsDataTypeIgnoringUserStartNodes(queryStrings?.GetValue<Guid?>("dataTypeId"));
            if (ignoreUserStartNodes == false)
            {
                int[]? aids = null;
                switch (entityType)
                {
                    case UmbracoEntityTypes.Document:
                        aids = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?
                            .CalculateContentStartNodeIds(_entityService, _appCaches);
                        break;
                    case UmbracoEntityTypes.Media:
                        aids =
                            _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.CalculateMediaStartNodeIds(
                                _entityService, _appCaches);
                        break;
                }

                if (aids != null)
                {
                    var lids = new List<int>();
                    var ok = false;
                    if (ids is not null)
                    {
                        foreach (var i in ids)
                        {
                            if (ok)
                            {
                                lids.Add(i);
                                continue;
                            }

                            if (aids.Contains(i))
                            {
                                lids.Add(i);
                                ok = true;
                            }
                        }
                    }

                    ids = lids.ToArray();
                }
            }

            var culture = queryStrings?.GetValue<string>("culture");

            return ids is null || ids.Length == 0
                ? Enumerable.Empty<EntityBasic>()
                : _entityService.GetAll(objectType.Value, ids)
                    .OrderBy(x => x.Level)
                    .Select(MapEntities(culture))
                    .WhereNotNull();
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.PropertyType:
            case UmbracoEntityTypes.PropertyGroup:
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private IEnumerable<EntityBasic> GetResultForKeys(Guid[] keys, UmbracoEntityTypes entityType)
    {
        if (keys.Length == 0)
        {
            return Enumerable.Empty<EntityBasic>();
        }

        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            IEnumerable<EntityBasic> entities = _entityService.GetAll(objectType.Value, keys)
                .Select(MapEntities())
                .WhereNotNull();

            // entities are in "some" order, put them back in order
            var xref = entities.ToDictionary(x => x.Key);
            IEnumerable<EntityBasic> result = keys.Select(x => xref.ContainsKey(x) ? xref[x] : null)
                .WhereNotNull();

            return result;
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.PropertyType:
            case UmbracoEntityTypes.PropertyGroup:
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private IEnumerable<EntityBasic> GetResultForIds(int[] ids, UmbracoEntityTypes entityType)
    {
        if (ids.Length == 0)
        {
            return Enumerable.Empty<EntityBasic>();
        }

        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            IEnumerable<EntityBasic> entities = _entityService.GetAll(objectType.Value, ids)
                .Select(MapEntities())
                .WhereNotNull();

            // entities are in "some" order, put them back in order
            var xref = entities.Where(x => x.Id != null).ToDictionary(x => x.Id!);
            IEnumerable<EntityBasic> result = ids.Select(x => xref.ContainsKey(x) ? xref[x] : null)
                .WhereNotNull();

            return result;
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.PropertyType:
            case UmbracoEntityTypes.PropertyGroup:
            case UmbracoEntityTypes.Language:
            case UmbracoEntityTypes.User:
            case UmbracoEntityTypes.Macro:
            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private ActionResult<EntityBasic?> GetResultForKey(Guid key, UmbracoEntityTypes entityType)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            IEntitySlim? found = _entityService.Get(key, objectType.Value);
            if (found == null)
            {
                return NotFound();
            }

            return _umbracoMapper.Map<IEntitySlim, EntityBasic>(found);
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.PropertyType:

            case UmbracoEntityTypes.PropertyGroup:

            case UmbracoEntityTypes.Language:

            case UmbracoEntityTypes.User:

            case UmbracoEntityTypes.Macro:

            case UmbracoEntityTypes.Template:
                ITemplate? template = _fileService.GetTemplate(key);
                if (template is null)
                {
                    return NotFound();
                }

                return _umbracoMapper.Map<ITemplate, EntityBasic>(template);

            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private ActionResult<EntityBasic?> GetResultForId(int id, UmbracoEntityTypes entityType)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            IEntitySlim? found = _entityService.Get(id, objectType.Value);
            if (found == null)
            {
                return NotFound();
            }

            return MapEntity(found);
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.PropertyType:

            case UmbracoEntityTypes.PropertyGroup:

            case UmbracoEntityTypes.Language:

            case UmbracoEntityTypes.User:

            case UmbracoEntityTypes.Macro:

            case UmbracoEntityTypes.Template:
                ITemplate? template = _fileService.GetTemplate(id);
                if (template is null)
                {
                    return NotFound();
                }

                return _umbracoMapper.Map<ITemplate, EntityBasic>(template);

            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private static UmbracoObjectTypes? ConvertToObjectType(UmbracoEntityTypes entityType)
    {
        switch (entityType)
        {
            case UmbracoEntityTypes.Document:
                return UmbracoObjectTypes.Document;
            case UmbracoEntityTypes.Media:
                return UmbracoObjectTypes.Media;
            case UmbracoEntityTypes.MemberType:
                return UmbracoObjectTypes.MemberType;
            case UmbracoEntityTypes.MemberGroup:
                return UmbracoObjectTypes.MemberGroup;
            case UmbracoEntityTypes.MediaType:
                return UmbracoObjectTypes.MediaType;
            case UmbracoEntityTypes.DocumentType:
                return UmbracoObjectTypes.DocumentType;
            case UmbracoEntityTypes.Member:
                return UmbracoObjectTypes.Member;
            case UmbracoEntityTypes.DataType:
                return UmbracoObjectTypes.DataType;
            default:
                //There is no UmbracoEntity conversion (things like Macros, Users, etc...)
                return null;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="type">The type of entity.</param>
    /// <param name="postFilter">
    ///     Optional filter - Format like: "BoolVariable==true&IntVariable>=6". Invalid filters are
    ///     ignored.
    /// </param>
    /// <returns></returns>
    public IEnumerable<EntityBasic>? GetAll(UmbracoEntityTypes type, string postFilter) =>
        GetResultForAll(type, postFilter);

    /// <summary>
    ///     Gets the result for the entity list based on the type
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="postFilter">A string where filter that will filter the results dynamically with linq - optional</param>
    /// <returns></returns>
    private IEnumerable<EntityBasic>? GetResultForAll(UmbracoEntityTypes entityType, string? postFilter = null)
    {
        UmbracoObjectTypes? objectType = ConvertToObjectType(entityType);
        if (objectType.HasValue)
        {
            // TODO: Should we order this by something ?
            IEnumerable<EntityBasic> entities =
                _entityService.GetAll(objectType.Value).Select(MapEntities()).WhereNotNull();
            return ExecutePostFilter(entities, postFilter);
        }

        //now we need to convert the unknown ones
        switch (entityType)
        {
            case UmbracoEntityTypes.Template:
                IEnumerable<ITemplate>? templates = _fileService.GetTemplates();
                IEnumerable<ITemplate>? filteredTemplates = ExecutePostFilter(templates, postFilter);
                return filteredTemplates?.Select(MapEntities()).WhereNotNull();

            case UmbracoEntityTypes.Macro:
                //Get all macros from the macro service
                IOrderedEnumerable<IMacro> macros = _macroService.GetAll().WhereNotNull().OrderBy(x => x.Name);
                IEnumerable<IMacro>? filteredMacros = ExecutePostFilter(macros, postFilter);
                return filteredMacros?.Select(MapEntities()).WhereNotNull();

            case UmbracoEntityTypes.PropertyType:

                //get all document types, then combine all property types into one list
                IEnumerable<IPropertyType> propertyTypes = _contentTypeService.GetAll()
                    .Cast<IContentTypeComposition>()
                    .Concat(_mediaTypeService.GetAll())
                    .ToArray()
                    .SelectMany(x => x.PropertyTypes)
                    .DistinctBy(composition => composition.Alias);
                IEnumerable<IPropertyType>? filteredPropertyTypes = ExecutePostFilter(propertyTypes, postFilter);
                return _umbracoMapper
                    .MapEnumerable<IPropertyType, EntityBasic>(filteredPropertyTypes ??
                                                               Enumerable.Empty<IPropertyType>()).WhereNotNull();

            case UmbracoEntityTypes.PropertyGroup:

                //get all document types, then combine all property types into one list
                IEnumerable<PropertyGroup> propertyGroups = _contentTypeService.GetAll()
                    .Cast<IContentTypeComposition>()
                    .Concat(_mediaTypeService.GetAll())
                    .ToArray()
                    .SelectMany(x => x.PropertyGroups)
                    .DistinctBy(composition => composition.Name);
                IEnumerable<PropertyGroup>? filteredpropertyGroups = ExecutePostFilter(propertyGroups, postFilter);
                return _umbracoMapper
                    .MapEnumerable<PropertyGroup, EntityBasic>(filteredpropertyGroups ??
                                                               Enumerable.Empty<PropertyGroup>()).WhereNotNull();

            case UmbracoEntityTypes.User:

                IEnumerable<IUser> users = _userService.GetAll(0, int.MaxValue, out _);
                IEnumerable<IUser>? filteredUsers = ExecutePostFilter(users, postFilter);
                return _umbracoMapper.MapEnumerable<IUser, EntityBasic>(filteredUsers ?? Enumerable.Empty<IUser>())
                    .WhereNotNull();

            case UmbracoEntityTypes.Stylesheet:

                if (!postFilter.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("Filtering on stylesheets is not currently supported");
                }

                return _fileService.GetStylesheets().Select(MapEntities()).WhereNotNull();

            case UmbracoEntityTypes.Script:

                if (!postFilter.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("Filtering on scripts is not currently supported");
                }

                return _fileService.GetScripts().Select(MapEntities()).WhereNotNull();

            case UmbracoEntityTypes.PartialView:

                if (!postFilter.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("Filtering on partial views is not currently supported");
                }

                return _fileService.GetPartialViews().Select(MapEntities()).WhereNotNull();

            case UmbracoEntityTypes.Language:

                if (!postFilter.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("Filtering on languages is not currently supported");
                }

                return _localizationService.GetAllLanguages().Select(MapEntities()).WhereNotNull();
            case UmbracoEntityTypes.DictionaryItem:

                if (!postFilter.IsNullOrWhiteSpace())
                {
                    throw new NotSupportedException("Filtering on dictionary items is not currently supported");
                }

                return GetAllDictionaryItems();

            default:
                throw new NotSupportedException("The " + typeof(EntityController) +
                                                " does not currently support data for the type " + entityType);
        }
    }

    private IEnumerable<T>? ExecutePostFilter<T>(IEnumerable<T>? entities, string? postFilter)
    {
        if (postFilter.IsNullOrWhiteSpace())
        {
            return entities;
        }

        var postFilterConditions = postFilter!.Split(Constants.CharArrays.Ampersand);

        foreach (var postFilterCondition in postFilterConditions)
        {
            QueryCondition? queryCondition = BuildQueryCondition<T>(postFilterCondition);

            if (queryCondition != null)
            {
                Expression<Func<T, bool>> whereClauseExpression = queryCondition.BuildCondition<T>("x");

                entities = entities?.Where(whereClauseExpression.Compile());
            }
        }

        return entities;
    }

    private static QueryCondition? BuildQueryCondition<T>(string postFilter)
    {
        var postFilterParts = postFilter.Split(_postFilterSplitStrings, 2, StringSplitOptions.RemoveEmptyEntries);

        if (postFilterParts.Length != 2)
        {
            return null;
        }

        var propertyName = postFilterParts[0];
        var constraintValue = postFilterParts[1];
        var stringOperator = postFilter.Substring(propertyName.Length, postFilter.Length - propertyName.Length - constraintValue.Length);
        Operator binaryOperator;

        try
        {
            binaryOperator = OperatorFactory.FromString(stringOperator);
        }
        catch (ArgumentException)
        {
            // unsupported operators are ignored
            return null;
        }

        Type type = typeof(T);
        PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            return null;
        }

        var queryCondition = new QueryCondition
        {
            Term = new OperatorTerm { Operator = binaryOperator },
            ConstraintValue = constraintValue,
            Property = new PropertyModel
            {
                Alias = propertyName,
                Name = propertyName,
                Type = property.PropertyType.Name
            }
        };

        return queryCondition;
    }

    private Func<object, EntityBasic?> MapEntities(string? culture = null)
    {
        culture ??= ClientCulture();
        return x => MapEntity(x, culture);
    }

    private EntityBasic? MapEntity(object entity, string? culture = null)
    {
        culture ??= ClientCulture();
        return _umbracoMapper.Map<EntityBasic>(entity, context => { context.SetCulture(culture); });
    }

    private string? ClientCulture() => Request.ClientCulture();


    #region GetById

    /// <summary>
    ///     Gets an entity by it's id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult<EntityBasic?> GetById(int id, UmbracoEntityTypes type) => GetResultForId(id, type);

    /// <summary>
    ///     Gets an entity by it's key
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult<EntityBasic?> GetById(Guid id, UmbracoEntityTypes type) => GetResultForKey(id, type);

    /// <summary>
    ///     Gets an entity by it's UDI
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public ActionResult<EntityBasic?> GetById(Udi id, UmbracoEntityTypes type)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi != null)
        {
            return GetResultForKey(guidUdi.Guid, type);
        }

        return NotFound();
    }

    #endregion

    #region GetByIds

    /// <summary>
    ///     Get entities by integer ids
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public ActionResult<IEnumerable<EntityBasic>> GetByIds([FromJsonPath] int[] ids, [FromQuery] UmbracoEntityTypes type)
    {
        if (ids == null)
        {
            return NotFound();
        }

        return new ActionResult<IEnumerable<EntityBasic>>(GetResultForIds(ids, type));
    }

    /// <summary>
    ///     Get entities by GUID ids
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public ActionResult<IEnumerable<EntityBasic>> GetByIds([FromJsonPath] Guid[] ids, [FromQuery] UmbracoEntityTypes type)
    {
        if (ids == null)
        {
            return NotFound();
        }

        return new ActionResult<IEnumerable<EntityBasic>>(GetResultForKeys(ids, type));
    }

    /// <summary>
    ///     Get entities by UDIs
    /// </summary>
    /// <param name="ids">
    ///     A list of UDIs to lookup items by, all UDIs must be of the same UDI type!
    /// </param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     We allow for POST because there could be quite a lot of Ids.
    /// </remarks>
    [HttpGet]
    [HttpPost]
    public ActionResult<IEnumerable<EntityBasic>> GetByIds([FromJsonPath] Udi[] ids, [FromQuery] UmbracoEntityTypes type)
    {
        if (ids == null)
        {
            return NotFound();
        }

        if (ids.Length == 0)
        {
            return Enumerable.Empty<EntityBasic>().ToList();
        }

        //all udi types will need to be the same in this list so we'll determine by the first
        //currently we only support GuidUdi for this method

        var guidUdi = ids[0] as GuidUdi;
        if (guidUdi != null)
        {
            return new ActionResult<IEnumerable<EntityBasic>>(
                GetResultForKeys(ids.Select(x => ((GuidUdi)x).Guid).ToArray(), type));
        }

        return NotFound();
    }

    #endregion

    #region Methods to get all dictionary items

    private IEnumerable<EntityBasic> GetAllDictionaryItems()
    {
        var list = new List<EntityBasic>();

        IEnumerable<IDictionaryItem>? rootDictionaryItems = _localizationService.GetRootDictionaryItems();
        if (rootDictionaryItems is not null)
        {
            foreach (IDictionaryItem dictionaryItem in rootDictionaryItems
                         .OrderBy(DictionaryItemSort()))
            {
                EntityBasic? item = _umbracoMapper.Map<IDictionaryItem, EntityBasic>(dictionaryItem);
                if (item is not null)
                {
                    list.Add(item);
                }

                GetChildItemsForList(dictionaryItem, list);
            }
        }

        return list;
    }

    private static Func<IDictionaryItem, string> DictionaryItemSort() => item => item.ItemKey;

    private void GetChildItemsForList(IDictionaryItem dictionaryItem, ICollection<EntityBasic> list)
    {
        IEnumerable<IDictionaryItem>? itemChildren = _localizationService.GetDictionaryItemChildren(dictionaryItem.Key);

        if (itemChildren is not null)
        {
            foreach (IDictionaryItem childItem in itemChildren
                         .OrderBy(DictionaryItemSort()))
            {
                EntityBasic? item = _umbracoMapper.Map<IDictionaryItem, EntityBasic>(childItem);
                if (item is not null)
                {
                    list.Add(item);
                }

                GetChildItemsForList(childItem, list);
            }
        }
    }

    #endregion
}
