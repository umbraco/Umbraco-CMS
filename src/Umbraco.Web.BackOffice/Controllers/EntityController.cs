﻿using System;
using System.Collections.Generic;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Xml;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.ModelBinders;
using Umbraco.Web.Models;
using Umbraco.Web.Models.TemplateQuery;
using Umbraco.Web.Search;
using Umbraco.Web.Services;
using Umbraco.Web.Trees;
using Constants = Umbraco.Core.Constants;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are based on CMSNode
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller allows resolving basic entity data for various entities without placing the hard restrictions on users that may not have access
    /// to the sections these entities entities exist in. This is to allow pickers, etc... of data to work for all users. In some cases such as accessing
    /// Members, more explicit security checks are done.
    /// </para>
    /// <para>Some objects such as macros are not based on CMSNode</para>
    /// </remarks>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        private readonly ITreeService _treeService;
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly SearchableTreeCollection _searchableTreeCollection;
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IEntityService _entityService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IContentService _contentService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IDataTypeService _dataTypeService;
        private readonly ISqlContext _sqlContext;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IFileService _fileService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMacroService _macroService;
        private readonly IUserService _userService;
        private readonly ILocalizationService _localizationService;

        public EntityController(
            ITreeService treeService,
            UmbracoTreeSearcher treeSearcher,
            SearchableTreeCollection searchableTreeCollection,
            IPublishedContentQuery publishedContentQuery,
            IShortStringHelper shortStringHelper,
            IEntityService entityService,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            IPublishedUrlProvider publishedUrlProvider,
            IContentService contentService,
            UmbracoMapper umbracoMapper,
            IDataTypeService dataTypeService,
            ISqlContext sqlContext,
            ILocalizedTextService localizedTextService,
            IFileService fileService,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMacroService macroService,
            IUserService userService,
            ILocalizationService localizationService)
        {
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            _treeSearcher = treeSearcher ?? throw new ArgumentNullException(nameof(treeSearcher));
            _searchableTreeCollection = searchableTreeCollection ??
                                        throw new ArgumentNullException(nameof(searchableTreeCollection));
            _publishedContentQuery =
                publishedContentQuery ?? throw new ArgumentNullException(nameof(publishedContentQuery));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
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
        }

        /// <summary>
        /// Returns an Umbraco alias given a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public dynamic GetSafeAlias(string value, bool camelCase = true)
        {
            var returnValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToSafeAlias(_shortStringHelper, camelCase);
            dynamic returnObj = new System.Dynamic.ExpandoObject();
            returnObj.alias = returnValue;
            returnObj.original = value;
            returnObj.camelCase = camelCase;

            return returnObj;
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="dataTypeKey">If set used to look up whether user and group start node permissions will be ignored.</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes type, string searchFrom = null, Guid? dataTypeKey = null)
        {
            // NOTE: Theoretically you shouldn't be able to see member data if you don't have access to members right? ... but there is a member picker, so can't really do that

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityBasic>();

            //TODO: This uses the internal UmbracoTreeSearcher, this instead should delgate to the ISearchableTree implementation for the type

            var ignoreUserStartNodes = dataTypeKey.HasValue && _dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeKey.Value);
            return ExamineSearch(query, type, searchFrom, ignoreUserStartNodes);
        }

        /// <summary>
        /// Searches for all content that the user is allowed to see (based on their allowed sections)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <remarks>
        /// Even though a normal entity search will allow any user to search on a entity type that they may not have access to edit, we need
        /// to filter these results to the sections they are allowed to edit since this search function is explicitly for the global search
        /// so if we showed entities that they weren't allowed to edit they would get errors when clicking on the result.
        ///
        /// The reason a user is allowed to search individual entity types that they are not allowed to edit is because those search
        /// methods might be used in things like pickers in the content editor.
        /// </remarks>
        [HttpGet]
        public IDictionary<string, TreeSearchResult> SearchAll(string query)
        {
            var result = new Dictionary<string, TreeSearchResult>();

            if (string.IsNullOrEmpty(query))
                return result;

            var allowedSections = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.AllowedSections.ToArray();

            foreach (var searchableTree in _searchableTreeCollection.SearchableApplicationTrees.OrderBy(t => t.Value.SortOrder))
            {
                if (allowedSections.Contains(searchableTree.Value.AppAlias))
                {
                    var tree = _treeService.GetByAlias(searchableTree.Key);
                    if (tree == null) continue; //shouldn't occur

                    result[Tree.GetRootNodeDisplayName(tree, _localizedTextService)] = new TreeSearchResult
                    {
                        Results = searchableTree.Value.SearchableTree.Search(query, 200, 0, out var total),
                        TreeAlias = searchableTree.Key,
                        AppAlias = searchableTree.Value.AppAlias,
                        JsFormatterService = searchableTree.Value.FormatterService,
                        JsFormatterMethod = searchableTree.Value.FormatterMethod
                    };
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<int> GetPath(int id, UmbracoEntityTypes type)
        {
            var foundContent = GetResultForId(id, type);

            return foundContent.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<int> GetPath(Guid id, UmbracoEntityTypes type)
        {
            var foundContent = GetResultForKey(id, type);

            return foundContent.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<int> GetPath(Udi id, UmbracoEntityTypes type)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetPath(guidUdi.Guid, type);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Gets the url of an entity
        /// </summary>
        /// <param name="udi">UDI of the entity to fetch URL for</param>
        /// <param name="culture">The culture to fetch the URL for</param>
        /// <returns>The URL or path to the item</returns>
       [DetermineAmbiguousActionByPassingParameters]
        public HttpResponseMessage GetUrl(Udi udi, string culture = "*")
        {
            var intId = _entityService.GetId(udi);
            if (!intId.Success)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            UmbracoEntityTypes entityType;
            switch (udi.EntityType)
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
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetUrl(intId.Result, entityType, culture);
        }

        /// <summary>
        /// Gets the url of an entity
        /// </summary>
        /// <param name="id">Int id of the entity to fetch URL for</param>
        /// <param name="type">The type of entity such as Document, Media, Member</param>
        /// <param name="culture">The culture to fetch the URL for</param>
        /// <returns>The URL or path to the item</returns>
        /// <remarks>
        /// We are not restricting this with security because there is no sensitive data
        /// </remarks>
        [DetermineAmbiguousActionByPassingParameters]
        public HttpResponseMessage GetUrl(int id, UmbracoEntityTypes type, string culture = null)
        {
            culture = culture ?? ClientCulture();

            var returnUrl = string.Empty;

            if (type == UmbracoEntityTypes.Document)
            {
                var foundUrl = _publishedUrlProvider.GetUrl(id, culture: culture);
                if (string.IsNullOrEmpty(foundUrl) == false && foundUrl != "#")
                {
                    returnUrl = foundUrl;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(returnUrl)
                    };
                }
            }

            var ancestors = GetResultForAncestors(id, type);

            //if content, skip the first node for replicating NiceUrl defaults
            if (type == UmbracoEntityTypes.Document)
            {
                ancestors = ancestors.Skip(1);
            }

            returnUrl = "/" + string.Join("/", ancestors.Select(x => x.Name));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(returnUrl)
            };
        }


        /// <summary>
        /// Gets an entity by a xpath query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeContextId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EntityBasic GetByQuery(string query, int nodeContextId, UmbracoEntityTypes type)
        {
            // TODO: Rename this!!! It's misleading, it should be GetByXPath


            if (type != UmbracoEntityTypes.Document)
                throw new ArgumentException("Get by query is only compatible with entities of type Document");


            var q = ParseXPathQuery(query, nodeContextId);
            var node = _publishedContentQuery.ContentSingleAtXPath(q);

            if (node == null)
                return null;

            return GetById(node.Id, type);
        }

        // PP: Work in progress on the query parser
        private string ParseXPathQuery(string query, int id)
        {
            return UmbracoXPathPathSyntaxParser.ParseXPathQuery(
                xpathExpression: query,
                nodeContextId: id,
                getPath: nodeid =>
                {
                    var ent = _entityService.Get(nodeid);
                    return ent.Path.Split(',').Reverse();
                },
                publishedContentExists: i => _publishedContentQuery.Content(i) != null);
        }

        [HttpGet]
        [DetermineAmbiguousActionByPassingParameters]
        public UrlAndAnchors GetUrlAndAnchors(Udi id, string culture = "*")
        {
            var intId = _entityService.GetId(id);
            if (!intId.Success)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return GetUrlAndAnchors(intId.Result, culture);
        }
        [HttpGet]
        [DetermineAmbiguousActionByPassingParameters]
        public UrlAndAnchors GetUrlAndAnchors(int id, string culture = "*")
        {
            culture = culture ?? ClientCulture();

            var url = _publishedUrlProvider.GetUrl(id, culture: culture);
            var anchorValues = _contentService.GetAnchorValuesFromRTEs(id, culture);
            return new UrlAndAnchors(url, anchorValues);
        }

        [HttpGet]
        [HttpPost]
        public IEnumerable<string> GetAnchors(AnchorsModel model)
        {
            var anchorValues = _contentService.GetAnchorValuesFromRTEContent(model.RteContent);
            return anchorValues;
        }


        #region GetById

        /// <summary>
        /// Gets an entity by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public EntityBasic GetById(int id, UmbracoEntityTypes type)
        {
            return GetResultForId(id, type);
        }

        /// <summary>
        /// Gets an entity by it's key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public EntityBasic GetById(Guid id, UmbracoEntityTypes type)
        {
            return GetResultForKey(id, type);
        }

        /// <summary>
        /// Gets an entity by it's UDI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public EntityBasic GetById(Udi id, UmbracoEntityTypes type)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetResultForKey(guidUdi.Guid, type);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
        #endregion

        #region GetByIds
        /// <summary>
        /// Get entities by integer ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]int[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForIds(ids, type);
        }

        /// <summary>
        /// Get entities by GUID ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]Guid[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForKeys(ids, type);
        }

        /// <summary>
        /// Get entities by UDIs
        /// </summary>
        /// <param name="ids">
        /// A list of UDIs to lookup items by, all UDIs must be of the same UDI type!
        /// </param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// We allow for POST because there could be quite a lot of Ids.
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [DetermineAmbiguousActionByPassingParameters]
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]Udi[] ids, [FromQuery]UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (ids.Length == 0)
            {
                return Enumerable.Empty<EntityBasic>();
            }

            //all udi types will need to be the same in this list so we'll determine by the first
            //currently we only support GuidIdi for this method

            var guidUdi = ids[0] as GuidUdi;
            if (guidUdi != null)
            {
                return GetResultForKeys(ids.Select(x => ((GuidUdi)x).Guid).ToArray(), type);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
        #endregion

        public IEnumerable<EntityBasic> GetChildren(int id, UmbracoEntityTypes type, Guid? dataTypeKey = null)
        {
            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support hierarchy here, some might not.

                var startNodes = GetStartNodes(type);

                var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);

                // root is special: we reduce it to start nodes if the user's start node is not the default, then we need to return their start nodes
                if (id == Constants.System.Root && startNodes.Length > 0 && startNodes.Contains(Constants.System.Root) == false && !ignoreUserStartNodes)
                {
                    var nodes = _entityService.GetAll(objectType.Value, startNodes).ToArray();
                    if (nodes.Length == 0)
                        return Enumerable.Empty<EntityBasic>();
                    var pr = new List<EntityBasic>(nodes.Select(_umbracoMapper.Map<EntityBasic>));
                    return pr;
                }

                // else proceed as usual

                return _entityService.GetChildren(id, objectType.Value)
                    .WhereNotNull()
                    .Select(_umbracoMapper.Map<EntityBasic>);
            }
            //now we need to convert the unknown ones
            switch (type)
            {
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + type);
            }
        }

        /// <summary>
        /// Get paged child entities by id
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
        [DetermineAmbiguousActionByPassingParameters]
        public PagedResult<EntityBasic> GetPagedChildren(
            string id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "",
            Guid? dataTypeKey = null)
        {
            if (int.TryParse(id, out var intId))
            {
                return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
            }

            if (Guid.TryParse(id, out _))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (UdiParser.TryParse(id, out _))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
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

            var searchResult = _treeSearcher.ExamineSearch(filter ?? "", type, pageSize, pageNumber - 1, out long total, id);

            return new PagedResult<EntityBasic>(total, pageNumber, pageSize)
            {
                Items = searchResult
            };
        }

        /// <summary>
        /// Get paged child entities by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [DetermineAmbiguousActionByPassingParameters]
        public PagedResult<EntityBasic> GetPagedChildren(
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
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                IEnumerable<IEntitySlim> entities;
                long totalRecords;

                var startNodes = GetStartNodes(type);

                var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);

                // root is special: we reduce it to start nodes if the user's start node is not the default, then we need to return their start nodes
                if (id == Constants.System.Root && startNodes.Length > 0 && startNodes.Contains(Constants.System.Root) == false && !ignoreUserStartNodes)
                {
                    if (pageNumber > 0)
                        return new PagedResult<EntityBasic>(0, 0, 0);
                    var nodes = _entityService.GetAll(objectType.Value, startNodes).ToArray();
                    if (nodes.Length == 0)
                        return new PagedResult<EntityBasic>(0, 0, 0);
                    if (pageSize < nodes.Length) pageSize = nodes.Length; // bah
                    var pr = new PagedResult<EntityBasic>(nodes.Length, pageNumber, pageSize)
                    {
                        Items = nodes.Select(_umbracoMapper.Map<EntityBasic>)
                    };
                    return pr;
                }

                // else proceed as usual
                entities = _entityService.GetPagedChildren(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                    filter.IsNullOrWhiteSpace()
                        ? null
                        : _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
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
                        var target = _umbracoMapper.Map<IEntitySlim, EntityBasic>(source, context =>
                        {
                            context.SetCulture(culture);
                            context.SetCulture(culture);
                        });
                        //TODO: Why is this here and not in the mapping?
                        target.AdditionalData["hasChildren"] = source.HasChildren;
                        return target;
                    })
                };

                return pagedResult;
            }

            //now we need to convert the unknown ones
            switch (type)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + type);
            }
        }

        private int[] GetStartNodes(UmbracoEntityTypes type)
        {
            switch (type)
            {
                case UmbracoEntityTypes.Document:
                    return _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateContentStartNodeIds(_entityService);
                case UmbracoEntityTypes.Media:
                    return _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService);
                default:
                    return Array.Empty<int>();
            }
        }

        public PagedResult<EntityBasic> GetPagedDescendants(
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
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            // re-normalize since NULL can be passed in
            filter = filter ?? string.Empty;

            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                IEnumerable<IUmbracoEntity> entities;
                long totalRecords;

                if (id == Constants.System.Root)
                {
                    // root is special: we reduce it to start nodes

                    int[] aids = GetStartNodes(type);

                    var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(dataTypeKey);
                    entities = aids == null || aids.Contains(Constants.System.Root) || ignoreUserStartNodes
                        ? _entityService.GetPagedDescendants(objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                            _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
                            Ordering.By(orderBy, orderDirection), includeTrashed: false)
                        : _entityService.GetPagedDescendants(aids, objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                            _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
                            Ordering.By(orderBy, orderDirection));
                }
                else
                {
                    entities = _entityService.GetPagedDescendants(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                        _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
                        Ordering.By(orderBy, orderDirection));
                }

                if (totalRecords == 0)
                {
                    return new PagedResult<EntityBasic>(0, 0, 0);
                }

                var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
                {
                    Items = entities.Select(MapEntities())
                };

                return pagedResult;
            }

            //now we need to convert the unknown ones
            switch (type)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + type);
            }
        }

        private bool IsDataTypeIgnoringUserStartNodes(Guid? dataTypeKey) => dataTypeKey.HasValue && _dataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeKey.Value);

        public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings)
        {
            return GetResultForAncestors(id, type, queryStrings);
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="searchFrom"></param>
        /// <param name="ignoreUserStartNodes">If set to true, user and group start node permissions will be ignored.</param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> ExamineSearch(string query, UmbracoEntityTypes entityType, string searchFrom = null, bool ignoreUserStartNodes = false)
        {
            var culture = ClientCulture();
            return _treeSearcher.ExamineSearch(query, entityType, 200, 0, out _, culture, searchFrom, ignoreUserStartNodes);
        }

        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                // TODO: Need to check for Object types that support hierarchic here, some might not.

                return _entityService.GetChildren(id, objectType.Value)
                    .WhereNotNull()
                    .Select(MapEntities());
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForAncestors(int id, UmbracoEntityTypes entityType, FormCollection queryStrings = null)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                // TODO: Need to check for Object types that support hierarchic here, some might not.

                var ids = _entityService.Get(id).Path.Split(',').Select(int.Parse).Distinct().ToArray();

                var ignoreUserStartNodes = IsDataTypeIgnoringUserStartNodes(queryStrings?.GetValue<Guid?>("dataTypeId"));
                if (ignoreUserStartNodes == false)
                {
                    int[] aids = null;
                    switch (entityType)
                    {
                        case UmbracoEntityTypes.Document:
                            aids = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateContentStartNodeIds(_entityService);
                            break;
                        case UmbracoEntityTypes.Media:
                            aids = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService);
                            break;
                    }

                    if (aids != null)
                    {
                        var lids = new List<int>();
                        var ok = false;
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
                        ids = lids.ToArray();
                    }
                }

                var culture = queryStrings?.GetValue<string>("culture");

                return ids.Length == 0
                    ? Enumerable.Empty<EntityBasic>()
                    : _entityService.GetAll(objectType.Value, ids)
                        .WhereNotNull()
                        .OrderBy(x => x.Level)
                        .Select(MapEntities(culture));
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForKeys(Guid[] keys, UmbracoEntityTypes entityType)
        {
            if (keys.Length == 0)
                return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = _entityService.GetAll(objectType.Value, keys)
                    .WhereNotNull()
                    .Select(MapEntities());

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Key);
                var result = keys.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

                return result;
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForIds(int[] ids, UmbracoEntityTypes entityType)
        {
            if (ids.Length == 0)
                return Enumerable.Empty<EntityBasic>();

            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var entities = _entityService.GetAll(objectType.Value, ids)
                    .WhereNotNull()
                    .Select(MapEntities());

                // entities are in "some" order, put them back in order
                var xref = entities.ToDictionary(x => x.Id);
                var result = ids.Select(x => xref.ContainsKey(x) ? xref[x] : null).Where(x => x != null);

                return result;
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:
                case UmbracoEntityTypes.PropertyGroup:
                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                case UmbracoEntityTypes.User:
                case UmbracoEntityTypes.Macro:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private EntityBasic GetResultForKey(Guid key, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var found = _entityService.Get(key, objectType.Value);
                if (found == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return _umbracoMapper.Map<IEntitySlim, EntityBasic>(found);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:

                case UmbracoEntityTypes.PropertyGroup:

                case UmbracoEntityTypes.Domain:

                case UmbracoEntityTypes.Language:

                case UmbracoEntityTypes.User:

                case UmbracoEntityTypes.Macro:

                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private EntityBasic GetResultForId(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                var found = _entityService.Get(id, objectType.Value);
                if (found == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return MapEntity(found);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.PropertyType:

                case UmbracoEntityTypes.PropertyGroup:

                case UmbracoEntityTypes.Domain:

                case UmbracoEntityTypes.Language:

                case UmbracoEntityTypes.User:

                case UmbracoEntityTypes.Macro:

                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
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
        ///
        /// </summary>
        /// <param name="type">The type of entity.</param>
        /// <param name="postFilter">Optional filter - Format like: "BoolVariable==true&IntVariable>=6". Invalid filters are ignored.</param>
        /// <returns></returns>
        public IEnumerable<EntityBasic> GetAll(UmbracoEntityTypes type, string postFilter)
        {
            return GetResultForAll(type, postFilter);
        }

        /// <summary>
        /// Gets the result for the entity list based on the type
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="postFilter">A string where filter that will filter the results dynamically with linq - optional</param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> GetResultForAll(UmbracoEntityTypes entityType, string postFilter = null)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                // TODO: Should we order this by something ?
                var entities = _entityService.GetAll(objectType.Value).WhereNotNull().Select(MapEntities());
                return ExecutePostFilter(entities, postFilter);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Template:
                    var templates = _fileService.GetTemplates();
                    var filteredTemplates = ExecutePostFilter(templates, postFilter);
                    return filteredTemplates.Select(MapEntities());

                case UmbracoEntityTypes.Macro:
                    //Get all macros from the macro service
                    var macros = _macroService.GetAll().WhereNotNull().OrderBy(x => x.Name);
                    var filteredMacros = ExecutePostFilter(macros, postFilter);
                    return filteredMacros.Select(MapEntities());

                case UmbracoEntityTypes.PropertyType:

                    //get all document types, then combine all property types into one list
                    var propertyTypes = _contentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(_mediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyTypes)
                                                .DistinctBy(composition => composition.Alias);
                    var filteredPropertyTypes = ExecutePostFilter(propertyTypes, postFilter);
                    return _umbracoMapper.MapEnumerable<IPropertyType, EntityBasic>(filteredPropertyTypes);

                case UmbracoEntityTypes.PropertyGroup:

                    //get all document types, then combine all property types into one list
                    var propertyGroups = _contentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(_mediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyGroups)
                                                .DistinctBy(composition => composition.Name);
                    var filteredpropertyGroups = ExecutePostFilter(propertyGroups, postFilter);
                    return _umbracoMapper.MapEnumerable<PropertyGroup, EntityBasic>(filteredpropertyGroups);

                case UmbracoEntityTypes.User:

                    var users = _userService.GetAll(0, int.MaxValue, out _);
                    var filteredUsers = ExecutePostFilter(users, postFilter);
                    return _umbracoMapper.MapEnumerable<IUser, EntityBasic>(filteredUsers);

                case UmbracoEntityTypes.Stylesheet:

                    if (!postFilter.IsNullOrWhiteSpace())
                        throw new NotSupportedException("Filtering on stylesheets is not currently supported");

                    return _fileService.GetStylesheets().Select(MapEntities());

                case UmbracoEntityTypes.Language:

                    if (!postFilter.IsNullOrWhiteSpace())
                        throw new NotSupportedException("Filtering on languages is not currently supported");

                    return _localizationService.GetAllLanguages().Select(MapEntities());
                case UmbracoEntityTypes.DictionaryItem:

                    if (!postFilter.IsNullOrWhiteSpace())
                        throw new NotSupportedException("Filtering on dictionary items is not currently supported");

                    return GetAllDictionaryItems();

                case UmbracoEntityTypes.Domain:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<T> ExecutePostFilter<T>(IEnumerable<T> entities, string postFilter)
        {
            if (postFilter.IsNullOrWhiteSpace()) return entities;

            var postFilterConditions = postFilter.Split('&');

            foreach (var postFilterCondition in postFilterConditions)
            {
                var queryCondition = BuildQueryCondition<T>(postFilterCondition);

                if (queryCondition != null)
                {
                    var whereClauseExpression = queryCondition.BuildCondition<T>("x");

                    entities = entities.Where(whereClauseExpression.Compile());
                }

            }
            return entities;
        }

        private static QueryCondition BuildQueryCondition<T>(string postFilter)
        {
            var postFilterParts = postFilter.Split(new[]
            {
                "=",
                "==",
                "!=",
                "<>",
                ">",
                "<",
                ">=",
                "<="
            }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (postFilterParts.Length != 2)
            {
                return null;
            }

            var propertyName = postFilterParts[0];
            var constraintValue = postFilterParts[1];
            var stringOperator = postFilter.Substring(propertyName.Length,
                postFilter.Length - propertyName.Length - constraintValue.Length);
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

            var type = typeof(T);
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                return null;
            }

            var queryCondition = new QueryCondition()
            {
                Term = new OperatorTerm()
                {
                    Operator = binaryOperator
                },
                ConstraintValue = constraintValue,
                Property = new PropertyModel()
                {
                    Alias = propertyName,
                    Name = propertyName,
                    Type = property.PropertyType.Name
                }
            };

            return queryCondition;
        }

        private Func<object, EntityBasic> MapEntities(string culture = null)
        {
            culture = culture ?? ClientCulture();
            return x => MapEntity(x, culture);
        }

        private EntityBasic MapEntity(object entity, string culture = null)
        {
            culture = culture ?? ClientCulture();
            return _umbracoMapper.Map<EntityBasic>(entity, context => { context.SetCulture(culture); });
        }

        private string ClientCulture() => Request.ClientCulture();

        #region Methods to get all dictionary items
        private IEnumerable<EntityBasic> GetAllDictionaryItems()
        {
            var list = new List<EntityBasic>();

            foreach (var dictionaryItem in _localizationService.GetRootDictionaryItems().OrderBy(DictionaryItemSort()))
            {
                var item = _umbracoMapper.Map<IDictionaryItem, EntityBasic>(dictionaryItem);
                list.Add(item);
                GetChildItemsForList(dictionaryItem, list);
            }

            return list;
        }

        private static Func<IDictionaryItem, string> DictionaryItemSort() => item => item.ItemKey;

        private void GetChildItemsForList(IDictionaryItem dictionaryItem, ICollection<EntityBasic> list)
        {
            foreach (var childItem in _localizationService.GetDictionaryItemChildren(dictionaryItem.Key).OrderBy(DictionaryItemSort()))
            {
                var item = _umbracoMapper.Map<IDictionaryItem, EntityBasic>(childItem);
                list.Add(item);

                GetChildItemsForList(childItem, list);
            }
        }
        #endregion

    }
}
