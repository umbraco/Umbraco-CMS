using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Web.Http;
using System.Net;
using Umbraco.Core.PropertyEditors;
using System;
using System.Net.Http;
using System.Text;
using Umbraco.Web.WebApi;
using ContentType = System.Net.Mime.ContentType;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.SchemaTypes)]
    [EnableOverrideAuthorization]
    public class SchemaTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SchemaTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public SchemaTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {

        }

        public int GetCount()
        {
            return Services.ContentTypeService.CountContentTypes();
        }

        public SchemaTypeDisplay GetById(int id)
        {
            var ct = Services.ContentTypeService.GetSchemaType(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<ISchemaType, SchemaTypeDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Deletes a document type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.ContentTypeService.GetSchemaType(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.ContentTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns the avilable compositions for this content type
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="filterContentTypes">
        /// This is normally an empty list but if additional content type aliases are passed in, any content types containing those aliases will be filtered out
        /// along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filterPropertyTypes">
        /// This is normally an empty list but if additional property type aliases are passed in, any content types that have these aliases will be filtered out.
        /// This is required because in the case of creating/modifying a content type because new property types being added to it are not yet persisted so cannot
        /// be looked up via the db, they need to be passed in.
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetAvailableCompositeSchemaTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId, UmbracoObjectTypes.SchemaType, filter.FilterContentTypes, filter.FilterPropertyTypes)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Request.CreateResponse(result);
        }

        public SchemaTypeDisplay GetEmpty(int parentId)
        {
            var ct = new SchemaType(parentId);
            ct.Icon = "icon-picture";

            var dto = Mapper.Map<ISchemaType, SchemaTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all member types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {

            return Services.ContentTypeService.GetAllSchemaTypes()
                               .Select(Mapper.Map<ISchemaType, ContentTypeBasic>);
        }

        /// <summary>
        /// Deletes a document type container wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteContainer(int id)
        {
            Services.ContentTypeService.DeleteSchemaTypeContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.ContentTypeService.CreateSchemaTypeContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public SchemaTypeDisplay PostSave(SchemaTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<ISchemaType, SchemaTypeDisplay, SchemaTypeSave, PropertyTypeBasic>(
                contentTypeSave: contentTypeSave,
                getContentType: i => Services.ContentTypeService.GetSchemaType(i),
                saveContentType: type => Services.ContentTypeService.Save(type));

            var display = Mapper.Map<SchemaTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/schemaTypeSavedHeader"),
                            string.Empty);

            return display;
        }

        /// <summary>
        /// Move the schema type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move,
                getContentType: i => Services.ContentTypeService.GetSchemaType(i),
                doMove: (type, i) => Services.ContentTypeService.MoveSchemaType(type, i));
        }

        /// <summary>
        /// Copy the schema type
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public HttpResponseMessage PostCopy(MoveOrCopy copy)
        {
            return PerformCopy(
                copy,
                getContentType: i => Services.ContentTypeService.GetSchemaType(i),
                doCopy: (type, i) => Services.ContentTypeService.CopySchemaType(type, i));
        }
    }
}