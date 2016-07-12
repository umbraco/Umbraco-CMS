using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Services;
using Umbraco.Core.PropertyEditors;
using System.Net.Http;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Strings;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Logging;
using Umbraco.Web.Models;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like
    //  GetAllowedChildren, GetPropertyTypeScaffold, GetAllPropertyTypeAliases are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
    [EnableOverrideAuthorization]
    public class ContentTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        public int GetCount()
        {
            return Services.ContentTypeService.CountContentTypes();
        }

        public DocumentTypeDisplay GetById(int id)
        {
            var ct = Services.ContentTypeService.GetContentType(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(ct);
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
            var foundType = Services.ContentTypeService.GetContentType(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.ContentTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets all user defined properties.
        /// </summary>
        /// <returns></returns>
        [UmbracoTreeAuthorize(
            Constants.Trees.DocumentTypes, Constants.Trees.Content,
            Constants.Trees.MediaTypes, Constants.Trees.Media,
            Constants.Trees.MemberTypes, Constants.Trees.Members)]
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            return ApplicationContext.Services.ContentTypeService.GetAllPropertyTypeAliases();
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
        public HttpResponseMessage GetAvailableCompositeContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId, UmbracoObjectTypes.DocumentType, filter.FilterContentTypes, filter.FilterPropertyTypes)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Request.CreateResponse(result);
        }

        [UmbracoTreeAuthorize(
            Constants.Trees.DocumentTypes, Constants.Trees.Content,
            Constants.Trees.MediaTypes, Constants.Trees.Media,
            Constants.Trees.MemberTypes, Constants.Trees.Members)]
        public ContentPropertyDisplay GetPropertyTypeScaffold(int id)
        {
            var dataTypeDiff = Services.DataTypeService.GetDataTypeDefinitionById(id);

            if (dataTypeDiff == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var preVals = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(id);
            var editor = PropertyEditorResolver.Current.GetByAlias(dataTypeDiff.PropertyEditorAlias);

            return new ContentPropertyDisplay()
            {
                Editor = dataTypeDiff.PropertyEditorAlias,
                Validation = new PropertyTypeValidation() { },
                View = editor.ValueEditor.View,
                Config = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals)
            };
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
            Services.ContentTypeService.DeleteContentTypeContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.ContentTypeService.CreateContentTypeContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public DocumentTypeDisplay PostSave(DocumentTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<IContentType, DocumentTypeDisplay, DocumentTypeSave, PropertyTypeBasic>(
                contentTypeSave:    contentTypeSave,
                getContentType:     i => Services.ContentTypeService.GetContentType(i),
                saveContentType:    type => Services.ContentTypeService.Save(type),
                beforeCreateNew:    ctSave =>
                {
                    //create a default template if it doesnt exist -but only if default template is == to the content type
                    if (ctSave.DefaultTemplate.IsNullOrWhiteSpace() == false && ctSave.DefaultTemplate == ctSave.Alias)
                    {
                        var template = Services.FileService.GetTemplate(ctSave.Alias);
                        if (template == null)
                        {
                            var tryCreateTemplate = Services.FileService.CreateTemplateForContentType(ctSave.Alias, ctSave.Name);
                            if (tryCreateTemplate == false)
                            {
                                Logger.Warn<ContentTypeController>(
                                    "Could not create a template for the Content Type: {0}, status: {1}",
                                    () => ctSave.Alias,
                                    () => tryCreateTemplate.Result.StatusType);
                            }
                            template = tryCreateTemplate.Result.Entity;
                        }

                        //make sure the template alias is set on the default and allowed template so we can map it back
                        ctSave.DefaultTemplate = template.Alias;

                    }
                });

            var display = Mapper.Map<DocumentTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/contentTypeSavedHeader"),
                            string.Empty);

            return display;
        }

        /// <summary>
        /// Returns an empty content type for use as a scaffold when creating a new type
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public DocumentTypeDisplay GetEmpty(int parentId)
        {
            IContentType ct;
            if (parentId != Constants.System.Root)
            {
                var parent = Services.ContentTypeService.GetContentType(parentId);
                ct = parent != null ? new ContentType(parent, string.Empty) : new ContentType(parentId);
            }
            else
                ct = new ContentType(parentId);

            ct.Icon = "icon-document";

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all content type objects
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {
            var types = Services.ContentTypeService.GetAllContentTypes();
            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>);

            return basics.Select(basic =>
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
                return basic;
            });
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes, Constants.Trees.Content)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IContentType> types;
            if (contentId == Constants.System.Root)
            {
                types = Services.ContentTypeService.GetAllContentTypes().ToList();

                //if no allowed root types are set, just return everything
                if (types.Any(x => x.AllowedAsRoot))
                    types = types.Where(x => x.AllowedAsRoot);
            }
            else
            {
                var contentItem = Services.ContentService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var ids = contentItem.ContentType.AllowedContentTypes.Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAllContentTypes(ids).ToList();
            }

            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>).ToList();

            var localizedTextService = Services.TextService;
            foreach (var basic in basics)
            {
                basic.Name = localizedTextService.UmbracoDictionaryTranslate(basic.Name);
                basic.Description = localizedTextService.UmbracoDictionaryTranslate(basic.Description);
            }

            return basics;
        }

        /// <summary>
        /// Move the content type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move,
                getContentType: i => Services.ContentTypeService.GetContentType(i),
                doMove: (type, i) => Services.ContentTypeService.MoveContentType(type, i));
        }

        /// <summary>
        /// Copy the content type
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public HttpResponseMessage PostCopy(MoveOrCopy copy)
        {
            return PerformCopy(
                copy,
                getContentType: i => Services.ContentTypeService.GetContentType(i),
                doCopy: (type, i) => Services.ContentTypeService.CopyContentType(type, i));
        }
    }
}