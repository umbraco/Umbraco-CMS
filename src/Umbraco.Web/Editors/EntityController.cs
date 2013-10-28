using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Examine.LuceneEngine;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Web.Dynamics;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are based on CMSNode
    /// </summary>
    /// <remarks>
    /// Some objects such as macros are not based on CMSNode
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {   
        [HttpGet]
        public ISearchResults Search(string query, UmbracoEntityTypes type)
        {
            if (string.IsNullOrEmpty(query))
                return SearchResults.Empty();

            return ExamineSearch(query, type);
        }

        [HttpGet]
        public IEnumerable<EntityTypeSearchResult> SearchAll(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityTypeSearchResult>();

            var contentResult = ExamineSearch(query, UmbracoEntityTypes.Document);
            var mediaResult = ExamineSearch(query, UmbracoEntityTypes.Media);
            var memberResult = ExamineSearch(query, UmbracoEntityTypes.Member);

            var result = new List<EntityTypeSearchResult>
                {
                    new EntityTypeSearchResult
                        {
                            EntityType = UmbracoEntityTypes.Document.ToString(),
                            Results = contentResult
                        },
                    new EntityTypeSearchResult
                        {
                            EntityType = UmbracoEntityTypes.Media.ToString(),
                            Results = mediaResult
                        },
                    new EntityTypeSearchResult
                        {
                            EntityType = UmbracoEntityTypes.Member.ToString(),
                            Results = memberResult
                        }
                };

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

        public EntityBasic GetById(int id, UmbracoEntityTypes type)
        {
            return GetResultForId(id, type);
        }

        public IEnumerable<EntityBasic> GetByIds([FromUri]int[] ids, UmbracoEntityTypes type)
        {
            if (ids == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return GetResultForIds(ids, type);
        }

        public IEnumerable<EntityBasic> GetChildren(int id, UmbracoEntityTypes type)
        {
            return GetResultForChildren(id, type);
        }

        public IEnumerable<EntityBasic> GetAncestors(int id, UmbracoEntityTypes type)
        {
            return GetResultForAncestors(id, type);
        }

        public IEnumerable<EntityBasic> GetAll(UmbracoEntityTypes type, string postFilter, [FromUri]IDictionary<string, object> postFilterParams)
        {
            return GetResultForAll(type, postFilter, postFilterParams);
        }
        
        private ISearchResults ExamineSearch(string query, UmbracoEntityTypes entityType)
        {
            var searcher = Constants.Examine.InternalSearcher;
            var type = "content";
            var fields = new[] { "id", "__nodeName", "bodyText" };
            
            //TODO: WE should really just allow passing in a lucene raw query
            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    searcher = Constants.Examine.InternalMemberSearcher;
                    type = "member";
                    fields = new[] { "id", "email", "loginName","nodeName"};
                    break;
                case UmbracoEntityTypes.Media:
                    type = "media";
                    break;
                case UmbracoEntityTypes.Document:
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " currently does not support searching against object type " + entityType);
                    
            }

            var internalSearcher = ExamineManager.Instance.SearchProviderCollection[searcher];
            var criteria = internalSearcher.CreateSearchCriteria(type, BooleanOperation.Or);
            
            var term = new[] { query.ToLower().Escape() };
            var operation = criteria.GroupedOr(fields, term).Compile();

            return internalSearcher.Search(operation);

        }

        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support heirarchy here, some might not.

                return Services.EntityService.GetChildren(id, objectType.Value).Select(Mapper.Map<EntityBasic>)
                    .WhereNotNull();
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

        private IEnumerable<EntityBasic> GetResultForAncestors(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support heirarchy here, some might not.

                var ids = Services.EntityService.Get(id).Path.Split(',').Select(int.Parse);
                return ids.Select(m => Mapper.Map<EntityBasic>(Services.EntityService.Get(m, objectType.Value)))
                    .WhereNotNull();
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
                
                //if a post filter is assigned then try to execute it
                if (postFilter.IsNullOrWhiteSpace() == false)
                {
                    return postFilterParams == null
                               ? entities.AsQueryable().Where(postFilter).ToArray()
                               : entities.AsQueryable().Where(postFilter, postFilterParams).ToArray();

                }
                return entities;
            }
            //now we need to convert the unknown ones
            switch (entityType)
            {
                case UmbracoEntityTypes.Macro:                    
                    //Get all macros from the macro service
                    var result = Services.MacroService.GetAll().WhereNotNull().OrderBy(x => x.Name).AsQueryable();

                    //if a post filter is assigned then try to execute it
                    if (postFilter.IsNullOrWhiteSpace() == false)
                    {
                        result = postFilterParams == null
                            ? result.Where(postFilter)
                            : result.Where(postFilter, postFilterParams);

                    }

                    return result.Select(Mapper.Map<EntityBasic>);

                case UmbracoEntityTypes.Domain:

                case UmbracoEntityTypes.Language:

                case UmbracoEntityTypes.User:

                default:
                    throw new NotSupportedException("The " + typeof(EntityController) + " does not currently support data for the type " + entityType);
            }
        }

        private IEnumerable<EntityBasic> GetResultForIds(IEnumerable<int> ids, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                return ids.Select(id => Mapper.Map<EntityBasic>(Services.EntityService.Get(id, objectType.Value)))
                          .WhereNotNull();
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
                case UmbracoEntityTypes.Template:
                    return UmbracoObjectTypes.Template;
                case UmbracoEntityTypes.MemberGroup:
                    return UmbracoObjectTypes.MemberGroup;
                case UmbracoEntityTypes.ContentItem:
                    return UmbracoObjectTypes.ContentItem;
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

    }
}
