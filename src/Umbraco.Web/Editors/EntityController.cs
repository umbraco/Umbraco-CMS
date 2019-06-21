﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Models.TemplateQuery;
using Umbraco.Web.Search;
using Umbraco.Web.Services;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;
namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are based on CMSNode
    /// </summary>
    /// <remarks>
    /// Some objects such as macros are not based on CMSNode
    /// </remarks>
    [EntityControllerConfiguration]
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        private readonly ITreeService _treeService;
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly SearchableTreeCollection _searchableTreeCollection;

        public EntityController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState,
            ITreeService treeService, UmbracoHelper umbracoHelper, SearchableTreeCollection searchableTreeCollection, UmbracoTreeSearcher treeSearcher)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _treeService = treeService;
            _searchableTreeCollection = searchableTreeCollection;
            _treeSearcher = treeSearcher;
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class EntityControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(

                    //This is a special case, we'll accept a String here so that we can get page members when the special "all-members"
                    //id is passed in eventually we'll probably want to support GUID + Udi too
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetPagedChildren", "id", typeof(int), typeof(string)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetPath", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetByIds", "ids", typeof(int[]), typeof(Guid[]), typeof(Udi[]))));
            }
        }

        /// <summary>
        /// Returns an Umbraco alias given a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public dynamic GetSafeAlias(string value, bool camelCase = true)
        {
            var returnValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToSafeAlias(camelCase);
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
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes type, string searchFrom = null)
        {
            // TODO: Should we restrict search results based on what app the user has access to?
            // - Theoretically you shouldn't be able to see member data if you don't have access to members right?

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityBasic>();

            //TODO: This uses the internal UmbracoTreeSearcher, this instead should delgate to the ISearchableTree implementation for the type

            return ExamineSearch(query, type, searchFrom);
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

            var allowedSections = Security.CurrentUser.AllowedSections.ToArray();

            foreach (var searchableTree in _searchableTreeCollection.SearchableApplicationTrees.OrderBy(t => t.Value.SortOrder))
            {
                if (allowedSections.Contains(searchableTree.Value.AppAlias))
                {
                    var tree = _treeService.GetByAlias(searchableTree.Key);
                    if (tree == null) continue; //shouldn't occur

                    result[Tree.GetRootNodeDisplayName(tree, Services.TextService)] = new TreeSearchResult
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
        public IEnumerable<int> GetPath(int id, UmbracoEntityTypes type)
        {
            var foundContent = GetResultForId(id, type);

            return foundContent.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        }

        /// <summary>
        /// Gets the path for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
        /// <param name="id">Int id of the entity to fetch URL for</param>
        /// <param name="type">The type of entity such as Document, Media, Member</param>
        /// <returns>The URL or path to the item</returns>
        public HttpResponseMessage GetUrl(int id, UmbracoEntityTypes type)
        {
            var returnUrl = string.Empty;

            if (type == UmbracoEntityTypes.Document)
            {
                var foundUrl = UmbracoContext.Url(id);
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
            if(type == UmbracoEntityTypes.Document) {
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
            var node = Umbraco.ContentSingleAtXPath(q);

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
                    var ent = Services.EntityService.Get(nodeid);
                    return ent.Path.Split(',').Reverse();
                },
                publishedContentExists: i => Umbraco.Content(i) != null);
        }

        #region GetById

        /// <summary>
        /// Gets an entity by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
        public IEnumerable<EntityBasic> GetByIds([FromJsonPath]Udi[] ids, [FromUri]UmbracoEntityTypes type)
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

        public IEnumerable<EntityBasic> GetChildren(int id, UmbracoEntityTypes type)
        {
            return GetResultForChildren(id, type);
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
        public PagedResult<EntityBasic> GetPagedChildren(
            string id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
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

            if (Udi.TryParse(id, out _))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //so we don't have an INT, GUID or UDI, it's just a string, so now need to check if it's a special id or a member type
            if (id == Constants.Conventions.MemberTypes.AllMembersListId)
            {
                //the EntityService can search paged members from the root

                intId = -1;
                return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
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
        public PagedResult<EntityBasic> GetPagedChildren(
            int id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            if (pageNumber <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                var entities = Services.EntityService.GetPagedChildren(id, objectType.Value, pageNumber - 1, pageSize, out var totalRecords,
                    filter.IsNullOrWhiteSpace()
                        ? null
                        : SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
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
                        var target = Mapper.Map<IEntitySlim, EntityBasic>(source, context =>
                        {
                            context.SetCulture(culture);
                        });
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

        public PagedResult<EntityBasic> GetPagedDescendants(
            int id,
            UmbracoEntityTypes type,
            int pageNumber,
            int pageSize,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            if (pageNumber <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (pageSize <= 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var objectType = ConvertToObjectType(type);
            if (objectType.HasValue)
            {
                IEnumerable<IUmbracoEntity> entities;
                long totalRecords;

                if (id == Constants.System.Root)
                {
                    // root is special: we reduce it to start nodes

                    int[] aids = null;
                    switch (type)
                    {
                        case UmbracoEntityTypes.Document:
                            aids = Security.CurrentUser.CalculateContentStartNodeIds(Services.EntityService);
                            break;
                        case UmbracoEntityTypes.Media:
                            aids = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService);
                            break;
                    }

                    entities = aids == null || aids.Contains(Constants.System.Root)
                        ? Services.EntityService.GetPagedDescendants(objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                            SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
                            Ordering.By(orderBy, orderDirection), includeTrashed: false)
                        : Services.EntityService.GetPagedDescendants(aids, objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                            SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
                            Ordering.By(orderBy, orderDirection));
                }
                else
                {
                    entities = Services.EntityService.GetPagedDescendants(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords,
                        SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(filter)),
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

        public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormDataCollection queryStrings)
        {
            return GetResultForAncestors(id, type, queryStrings);
        }

        /// <summary>
        /// Searches for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="searchFrom"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> ExamineSearch(string query, UmbracoEntityTypes entityType, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, entityType, 200, 0, out _, searchFrom);
        }

        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                // TODO: Need to check for Object types that support hierarchic here, some might not.

                return Services.EntityService.GetChildren(id, objectType.Value)
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

        private IEnumerable<EntityBasic> GetResultForAncestors(int id, UmbracoEntityTypes entityType, FormDataCollection queryStrings = null)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                // TODO: Need to check for Object types that support hierarchic here, some might not.

                var ids = Services.EntityService.Get(id).Path.Split(',').Select(int.Parse).Distinct().ToArray();

                int[] aids = null;
                switch (entityType)
                {
                    case UmbracoEntityTypes.Document:
                        aids = Security.CurrentUser.CalculateContentStartNodeIds(Services.EntityService);
                        break;
                    case UmbracoEntityTypes.Media:
                        aids = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService);
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

                var culture = queryStrings?.GetValue<string>("culture");

                return ids.Length == 0
                    ? Enumerable.Empty<EntityBasic>()
                    : Services.EntityService.GetAll(objectType.Value, ids)
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
                var entities = Services.EntityService.GetAll(objectType.Value, keys)
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
                var entities = Services.EntityService.GetAll(objectType.Value, ids)
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
                var found = Services.EntityService.Get(key, objectType.Value);
                if (found == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return Mapper.Map<IEntitySlim, EntityBasic>(found);
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
                var found = Services.EntityService.Get(id, objectType.Value);
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
                var entities = Services.EntityService.GetAll(objectType.Value).WhereNotNull().Select(MapEntities());
                return ExecutePostFilter(entities, postFilter);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Template:
                    var templates = Services.FileService.GetTemplates();
                    var filteredTemplates = ExecutePostFilter(templates, postFilter);
                    return filteredTemplates.Select(MapEntities());

                case UmbracoEntityTypes.Macro:
                    //Get all macros from the macro service
                    var macros = Services.MacroService.GetAll().WhereNotNull().OrderBy(x => x.Name);
                    var filteredMacros = ExecutePostFilter(macros, postFilter);
                    return filteredMacros.Select(MapEntities());

                case UmbracoEntityTypes.PropertyType:

                    //get all document types, then combine all property types into one list
                    var propertyTypes = Services.ContentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(Services.MediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyTypes)
                                                .DistinctBy(composition => composition.Alias);
                    var filteredPropertyTypes = ExecutePostFilter(propertyTypes, postFilter);
                    return Mapper.MapEnumerable<PropertyType, EntityBasic>(filteredPropertyTypes);

                case UmbracoEntityTypes.PropertyGroup:

                    //get all document types, then combine all property types into one list
                    var propertyGroups = Services.ContentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(Services.MediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyGroups)
                                                .DistinctBy(composition => composition.Name);
                    var filteredpropertyGroups = ExecutePostFilter(propertyGroups, postFilter);
                    return Mapper.MapEnumerable<PropertyGroup, EntityBasic>(filteredpropertyGroups);

                case UmbracoEntityTypes.User:

                    var users = Services.UserService.GetAll(0, int.MaxValue, out _);
                    var filteredUsers = ExecutePostFilter(users, postFilter);
                    return Mapper.MapEnumerable<IUser, EntityBasic>(filteredUsers);

                case UmbracoEntityTypes.Stylesheet:

                    if (!postFilter.IsNullOrWhiteSpace())
                        throw new NotSupportedException("Filtering on stylesheets is not currently supported");

                    return Services.FileService.GetStylesheets().Select(MapEntities());

                case UmbracoEntityTypes.Language:

                    if (!postFilter.IsNullOrWhiteSpace() )
                        throw new NotSupportedException("Filtering on languages is not currently supported");

                    return Services.LocalizationService.GetAllLanguages().Select(MapEntities());
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
            return Mapper.Map<EntityBasic>(entity, context => { context.SetCulture(culture); });
        }

        private string ClientCulture() => Request.ClientCulture();

        #region Methods to get all dictionary items
        private IEnumerable<EntityBasic> GetAllDictionaryItems()
        {
            var list = new List<EntityBasic>();

            foreach (var dictionaryItem in Services.LocalizationService.GetRootDictionaryItems().OrderBy(DictionaryItemSort()))
            {
                var item = Mapper.Map<IDictionaryItem, EntityBasic>(dictionaryItem);
                list.Add(item);
                GetChildItemsForList(dictionaryItem, list);
            }

            return list;
        }

        private static Func<IDictionaryItem, string> DictionaryItemSort() => item => item.ItemKey;

        private void GetChildItemsForList(IDictionaryItem dictionaryItem, ICollection<EntityBasic> list)
        {
            foreach (var childItem in Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key).OrderBy(DictionaryItemSort()))
            {
                var item = Mapper.Map<IDictionaryItem, EntityBasic>(childItem);
                list.Add(item);

                GetChildItemsForList(childItem, list);
            }
        }
        #endregion

    }
}
