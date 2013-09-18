using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Newtonsoft.Json;
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

namespace Umbraco.Web.Editors
{
    ///// <summary>
    ///// API controller to deal with Macro data
    ///// </summary>
    //[PluginController("UmbracoApi")]
    //public class MacroController : UmbracoAuthorizedJsonController
    //{
    //    public EntityBasic Get
    //}

    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of umbraco objects that are based on CMSNode
    /// </summary>
    /// <remarks>
    /// Some objects such as macros are not based on CMSNode
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class EntityController : UmbracoAuthorizedJsonController
    {
        //[EnsureUserPermissionForContent("id")]
        //[UmbracoApplicationAuthorize(Constants.Applications.Content)]
        //public EntityBasic GetDocumentById(int id)
        //{
        //    return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        //}

        //[EnsureUserPermissionForContent("id")]
        //[UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        //[FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        //public IEnumerable<EntityBasic> GetDocumentChildren(int id)
        //{
        //    return GetChildren(id, UmbracoObjectTypes.Document);
        //}

        //[FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        //[UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        //public IEnumerable<EntityBasic> GetDocumentsByIds([FromUri]int[] ids)
        //{
        //    if (ids == null) throw new ArgumentNullException("ids");
        //    return GetEntitiesById(ids, UmbracoObjectTypes.Document);
        //}

        //[FilterAllowedOutgoingContent(typeof(IEnumerable<EntityBasic>))]
        //[UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
        //public IEnumerable<EntityBasic> SearchDocuments([FromUri]string query)
        //{
        //    var internalSearcher = ExamineManager.Instance.SearchProviderCollection[Constants.Examine.InternalSearcher];
        //    var criteria = internalSearcher.CreateSearchCriteria("content", BooleanOperation.Or);
        //    var fields = new[] { "id", "__nodeName", "bodyText" };
        //    var term = new[] { query.ToLower().Escape() };
        //    var operation = criteria.GroupedOr(fields, term).Compile();

        //    var results = internalSearcher.Search(operation)
        //        .Select(x =>  int.Parse(x["id"]));

        //    return GetDocumentsByIds(results.ToArray());
        //}

        ///// <summary>
        ///// The user must have access to either content or media for this to return data
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[UmbracoApplicationAuthorizeAttribute(
        //    Constants.Applications.Media, 
        //    Constants.Applications.Content)]
        //[EnsureUserPermissionForMedia("id")]
        //public EntityBasic GetMediaById(int id)
        //{
        //    return GetEntityById(id, UmbracoObjectTypes.Media);
        //}

        ///// <summary>
        ///// The user must have access to either content or media for this to return data
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[UmbracoApplicationAuthorizeAttribute(
        //    Constants.Applications.Media,
        //    Constants.Applications.Content)]
        //[EnsureUserPermissionForMedia("id")]
        //[FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        //public IEnumerable<EntityBasic> GetMediaChildren(int id)
        //{
        //    return GetChildren(id, UmbracoObjectTypes.Media);
        //}


        ///// <summary>
        ///// The user must have access to either content or media for this to return data
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[UmbracoApplicationAuthorizeAttribute(
        //    Constants.Applications.Media,
        //    Constants.Applications.Content)]
        //[EnsureUserPermissionForMedia("id")]
        //[FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        //public IEnumerable<EntityBasic> SearchMedia([FromUri]string query)
        //{
        //    var internalSearcher = ExamineManager.Instance.SearchProviderCollection[Constants.Examine.InternalSearcher];
        //    var criteria = internalSearcher.CreateSearchCriteria("media", BooleanOperation.Or);
        //    var fields = new[] { "id", "__nodeName"};
        //    var term = new[] { query.ToLower().Escape() };
        //    var operation = criteria.GroupedOr(fields, term).Compile();

        //    var results = internalSearcher.Search(operation)
        //        .Select(x => int.Parse(x["id"]));

        //    return GetMediaByIds(results.ToArray());
        //}

        ///// <summary>
        ///// The user must have access to either content or media for this to return data
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <returns></returns>
        //[UmbracoApplicationAuthorizeAttribute(
        //    Constants.Applications.Media,
        //    Constants.Applications.Content)]
        //[FilterAllowedOutgoingMedia(typeof(IEnumerable<EntityBasic>))]
        //public IEnumerable<EntityBasic> GetMediaByIds([FromUri]int[] ids)
        //{
        //    if (ids == null) throw new ArgumentNullException("ids");
        //    return GetEntitiesById(ids, UmbracoObjectTypes.Media);
        //}

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

        public IEnumerable<EntityBasic> GetAll(UmbracoEntityTypes type)
        {
            return GetResultForAll(type);
        }

        private IEnumerable<EntityBasic> GetResultForChildren(int id, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                //TODO: Need to check for Object types that support heirarchy here, some might not.

                return Services.EntityService.GetChildren(id).Select(Mapper.Map<EntityBasic>);
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
                return ids.Select(m => Mapper.Map<EntityBasic>(Services.EntityService.Get(m)));
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

        private IEnumerable<EntityBasic> GetResultForAll(UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                return Services.EntityService.GetAll(objectType.Value).Select(Mapper.Map<EntityBasic>);
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

        private IEnumerable<EntityBasic> GetResultForIds(IEnumerable<int> ids, UmbracoEntityTypes entityType)
        {
            var objectType = ConvertToObjectType(entityType);
            if (objectType.HasValue)
            {
                return ids.Select(id => Mapper.Map<EntityBasic>(Services.EntityService.Get(id)));
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
                return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, objectType.Value));
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
