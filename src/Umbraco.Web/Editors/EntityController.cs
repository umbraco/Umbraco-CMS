using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using Umbraco.Core.Models;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using System.Web.Http.Controllers;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Xml;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Search;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

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

        private readonly UmbracoTreeSearcher _treeSearcher = new UmbracoTreeSearcher();
        private readonly SearchableTreeCollection _searchableTreeCollection;

        public EntityController(SearchableTreeCollection searchableTreeCollection)
        {
            _searchableTreeCollection = searchableTreeCollection;
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
            //TODO: Should we restrict search results based on what app the user has access to?
            // - Theoretically you shouldn't be able to see member data if you don't have access to members right?

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityBasic>();

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

            foreach (var searchableTree in _searchableTreeCollection.SearchableApplicationTrees)
            {
                if (allowedSections.Contains(searchableTree.Value.AppAlias))
                {
                    var tree = Services.ApplicationTreeService.GetByAlias(searchableTree.Key);
                    if (tree == null) continue; //shouldn't occur

                    var searchableTreeAttribute = searchableTree.Value.SearchableTree.GetType().GetCustomAttribute<SearchableTreeAttribute>(false);

                    result[tree.GetRootNodeDisplayName(Services.TextService)] = new TreeSearchResult
                    {
                        Results = searchableTree.Value.SearchableTree.Search(query, 200, 0, out var total),
                        TreeAlias = searchableTree.Key,
                        AppAlias = searchableTree.Value.AppAlias,
                        JsFormatterService = searchableTreeAttribute == null ? "" : searchableTreeAttribute.ServiceName,
                        JsFormatterMethod = searchableTreeAttribute == null ? "" : searchableTreeAttribute.MethodName
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
        /// <param name="type">The tpye of entity such as Document, Media, Member</param>
        /// <returns>The URL or path to the item</returns>
        public HttpResponseMessage GetUrl(int id, UmbracoEntityTypes type)
        {
            var returnUrl = string.Empty;

            if (type == UmbracoEntityTypes.Document)
            {
                var foundUrl = Umbraco.Url(id);
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
            //TODO: Rename this!!! It's misleading, it should be GetByXPath


            if (type != UmbracoEntityTypes.Document)
                throw new ArgumentException("Get by query is only compatible with enitities of type Document");


            var q = ParseXPathQuery(query, nodeContextId);
            var node = Umbraco.ContentSingleAtXPath(q);

            if (node == null)
                return null;

            return GetById(node.Id, type);
        }

        //PP: wip in progress on the query parser
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
            int intId;

            if (int.TryParse(id, out intId))
            {
                return GetPagedChildren(intId, type, pageNumber, pageSize, orderBy, orderDirection, filter);
            }

            Guid guidId;
            if (Guid.TryParse(id, out guidId))
            {
                //Not supported currently
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Udi udiId;
            if (Udi.TryParse(id, out udiId))
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

            long total;
            var searchResult = _treeSearcher.ExamineSearch(Umbraco, filter ?? "", type, pageSize, pageNumber - 1, out total, id);

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
                long totalRecords;
                var entities = Services.EntityService.GetPagedChildren(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter);

                if (totalRecords == 0)
                {
                    return new PagedResult<EntityBasic>(0, 0, 0);
                }

                var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
                {
                    Items = entities.Select(Mapper.Map<EntityBasic>)
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
                        ? Services.EntityService.GetPagedDescendants(objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter, includeTrashed: false)
                        : Services.EntityService.GetPagedDescendants(aids, objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter);
                }
                else
                {
                    entities = Services.EntityService.GetPagedDescendants(id, objectType.Value, pageNumber - 1, pageSize, out totalRecords, orderBy, orderDirection, filter);
                }

                if (totalRecords == 0)
                {
                    return new PagedResult<EntityBasic>(0, 0, 0);
                }

                var pagedResult = new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
                {
                    Items = entities.Select(Mapper.Map<EntityBasic>)
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

        [HttpQueryStringFilter("queryStrings")]
        public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type, FormDataCollection queryStrings)
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
        private IEnumerable<SearchResultItem> ExamineSearch(string query, UmbracoEntityTypes entityType, string searchFrom = null)
        {
            long total;
            return _treeSearcher.ExamineSearch(Umbraco, query, entityType, 200, 0, out total, searchFrom);
        }




        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support hierarchic here, some might not.

                return Services.EntityService.GetChildren(id, objectType.Value)
                    .WhereNotNull()
                    .Select(Mapper.Map<EntityBasic>);
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
                //TODO: Need to check for Object types that support hierarchic here, some might not.

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
                        .Select(x => Mapper.Map<EntityBasic>(x, opts => { opts.SetCulture(culture);}));
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
                    .Select(Mapper.Map<EntityBasic>);

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
                    .Select(Mapper.Map<EntityBasic>);

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
                return Mapper.Map<EntityBasic>(found);
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
                return Mapper.Map<EntityBasic>(found);
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
                    return UmbracoObjectTypes.MediaType;
                case UmbracoEntityTypes.MemberGroup:
                    return UmbracoObjectTypes.MemberGroup;
                case UmbracoEntityTypes.MediaType:
                    return UmbracoObjectTypes.MediaType;
                case UmbracoEntityTypes.DocumentType:
                    return UmbracoObjectTypes.DocumentType;
                case UmbracoEntityTypes.Stylesheet:
                    return UmbracoObjectTypes.Stylesheet;
                case UmbracoEntityTypes.Member:
                    return UmbracoObjectTypes.Member;
                case UmbracoEntityTypes.DataType:
                    return UmbracoObjectTypes.DataType;
                default:
                    //There is no UmbracoEntity conversion (things like Macros, Users, etc...)
                    return null;
            }
        }

        // fixme - need to implement GetAll for backoffice controllers - dynamics?

        public IEnumerable<EntityBasic> GetAll(UmbracoEntityTypes type, string postFilter, [FromUri]IDictionary<string, object> postFilterParams)
        {
            return GetResultForAll(type, postFilter, postFilterParams);
        }

        /// <summary>
        /// Gets the result for the entity list based on the type
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="postFilter">A string where filter that will filter the results dynamically with linq - optional</param>
        /// <param name="postFilterParams">the parameters to fill in the string where filter - optional</param>
        /// <returns></returns>
        private IEnumerable<EntityBasic> GetResultForAll(UmbracoEntityTypes entityType, string postFilter = null, IDictionary<string, object> postFilterParams = null)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Should we order this by something ?
                var entities = Services.EntityService.GetAll(objectType.Value).WhereNotNull().Select(Mapper.Map<EntityBasic>);
                return ExecutePostFilter(entities, postFilter, postFilterParams);
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Template:
                    var templates = Services.FileService.GetTemplates();
                    var filteredTemplates = ExecutePostFilter(templates, postFilter, postFilterParams);
                    return filteredTemplates.Select(Mapper.Map<EntityBasic>);

                case UmbracoEntityTypes.Macro:
                    //Get all macros from the macro service
                    var macros = Services.MacroService.GetAll().WhereNotNull().OrderBy(x => x.Name);
                    var filteredMacros = ExecutePostFilter(macros, postFilter, postFilterParams);
                    return filteredMacros.Select(Mapper.Map<EntityBasic>);

                case UmbracoEntityTypes.PropertyType:

                    //get all document types, then combine all property types into one list
                    var propertyTypes = Services.ContentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(Services.MediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyTypes)
                                                .DistinctBy(composition => composition.Alias);
                    var filteredPropertyTypes = ExecutePostFilter(propertyTypes, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<PropertyType>, IEnumerable<EntityBasic>>(filteredPropertyTypes);

                case UmbracoEntityTypes.PropertyGroup:

                    //get all document types, then combine all property types into one list
                    var propertyGroups = Services.ContentTypeService.GetAll().Cast<IContentTypeComposition>()
                                                .Concat(Services.MediaTypeService.GetAll())
                                                .ToArray()
                                                .SelectMany(x => x.PropertyGroups)
                                                .DistinctBy(composition => composition.Name);
                    var filteredpropertyGroups = ExecutePostFilter(propertyGroups, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<PropertyGroup>, IEnumerable<EntityBasic>>(filteredpropertyGroups);

                case UmbracoEntityTypes.User:

                    long total;
                    var users = Services.UserService.GetAll(0, int.MaxValue, out total);
                    var filteredUsers = ExecutePostFilter(users, postFilter, postFilterParams);
                    return Mapper.Map<IEnumerable<IUser>, IEnumerable<EntityBasic>>(filteredUsers);

                case UmbracoEntityTypes.Domain:
                case UmbracoEntityTypes.Language:
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<T> ExecutePostFilter<T>(IEnumerable<T> entities, string postFilter, IDictionary<string, object> postFilterParams)
        {
            // if a post filter is assigned then try to execute it
            if (postFilter.IsNullOrWhiteSpace() == false)
            {
                // fixme - trouble is, we've killed the dynamic Where thing!
                throw new NotImplementedException("oops");
                //return postFilterParams == null
                //               ? entities.AsQueryable().Where(postFilter).ToArray()
                //               : entities.AsQueryable().Where(postFilter, postFilterParams).ToArray();
            }
            return entities;
        }
    }
}
