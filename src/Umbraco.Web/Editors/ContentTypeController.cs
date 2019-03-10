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
        /// Gets all the standard fields.
        /// </summary>
        /// <returns></returns>
        [UmbracoTreeAuthorize(
            Constants.Trees.DocumentTypes, Constants.Trees.Content,
            Constants.Trees.MediaTypes, Constants.Trees.Media,
            Constants.Trees.MemberTypes, Constants.Trees.Members)]
        public IEnumerable<string> GetAllStandardFields()
        {
            string[] preValuesSource = { "createDate", "creatorName", "level", "nodeType", "nodeTypeAlias", "pageID", "pageName", "parentID", "path", "template", "updateDate", "writerID", "writerName" };
            return preValuesSource;
        }

        /// <summary>
        /// Returns the avilable compositions for this content type
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="filter"></param>
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
        /// <summary>
        /// Returns where a particular composition has been used
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetWhereCompositionIsUsedInContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetWhereCompositionIsUsedInContentTypes(filter.ContentTypeId, UmbracoObjectTypes.DocumentType)
                .Select(x => new
                {
                    contentType = x
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

        public HttpResponseMessage PostRenameContainer(int id, string name)
        {
            var result = Services.ContentTypeService.RenameContentTypeContainer(id, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }
        
        public DocumentTypeCollectionDisplay PostCreateCollection(int parentId, string collectionName, bool collectionCreateTemplate, string collectionItemName, bool collectionItemCreateTemplate, string collectionIcon, string collectionItemIcon)
        {
            // create item doctype
            var itemDocType = new ContentType(parentId);
            itemDocType.Name = collectionItemName;
            itemDocType.Alias = collectionItemName.ToSafeAlias(true);
            itemDocType.Icon = collectionItemIcon;
            
            // create item doctype template
            if (collectionItemCreateTemplate)
            {
                var template = CreateTemplateForContentType(itemDocType.Alias, itemDocType.Name);
                itemDocType.SetDefaultTemplate(template);
            }

            // save item doctype
            Services.ContentTypeService.Save(itemDocType);

            // create collection doctype
            var collectionDocType = new ContentType(parentId);
            collectionDocType.Name = collectionName;
            collectionDocType.Alias = collectionName.ToSafeAlias(true);
            collectionDocType.Icon = collectionIcon;
            collectionDocType.IsContainer = true;
            collectionDocType.AllowedContentTypes = new List<ContentTypeSort>()
            {
                new ContentTypeSort(itemDocType.Id, 0)
            };
            
            // create collection doctype template
            if (collectionCreateTemplate)
            {
                var template = CreateTemplateForContentType(collectionDocType.Alias, collectionDocType.Name);
                collectionDocType.SetDefaultTemplate(template);
            }

            // save collection doctype
            Services.ContentTypeService.Save(collectionDocType);

            // test if the parent exist and then allow the collection underneath
            var parentCt = Services.ContentTypeService.GetContentType(parentId);
            if (parentCt != null)
            {
                var allowedCts = parentCt.AllowedContentTypes.ToList();
                allowedCts.Add(new ContentTypeSort(collectionDocType.Id, allowedCts.Count()));
                parentCt.AllowedContentTypes = allowedCts;
                Services.ContentTypeService.Save(parentCt);
            }

            return new DocumentTypeCollectionDisplay
            {
                CollectionId = collectionDocType.Id,
                ItemId = itemDocType.Id
            };
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
                        var template = CreateTemplateForContentType(ctSave.Alias, ctSave.Name);

                        // If the alias has been manually updated before the first save,
                        // make sure to also update the first allowed template, as the
                        // name will come back as a SafeAlias of the document type name,
                        // not as the actual document type alias.
                        // For more info: http://issues.umbraco.org/issue/U4-11059
                        if (ctSave.DefaultTemplate != template.Alias)
                        {
                            var allowedTemplates = ctSave.AllowedTemplates.ToArray();
                            if (allowedTemplates.Any())
                                allowedTemplates[0] = template.Alias;
                            ctSave.AllowedTemplates = allowedTemplates;
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

        private ITemplate CreateTemplateForContentType(string contentTypeAlias, string contentTypeName)
        {
            var template = Services.FileService.GetTemplate(contentTypeAlias);
            if (template == null)
            {
                var tryCreateTemplate = Services.FileService.CreateTemplateForContentType(contentTypeAlias, contentTypeName);
                if (tryCreateTemplate == false)
                {
                    Logger.Warn<ContentTypeController>(
                        "Could not create a template for the Content Type: {0}, status: {1}",
                        () => contentTypeAlias,
                        () => tryCreateTemplate.Result.StatusType);
                }

                template = tryCreateTemplate.Result.Entity;
            }

            return template;
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

                var ids = contentItem.ContentType.AllowedContentTypes.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAllContentTypes(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
            }

            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>).ToList();

            var localizedTextService = Services.TextService;
            foreach (var basic in basics)
            {
                basic.Name = localizedTextService.UmbracoDictionaryTranslate(basic.Name);
                basic.Description = localizedTextService.UmbracoDictionaryTranslate(basic.Description);
            }

            //map the blueprints
            var blueprints = Services.ContentService.GetBlueprintsForContentTypes(types.Select(x => x.Id).ToArray()).ToArray();
            foreach (var basic in basics)
            {
                var docTypeBluePrints = blueprints.Where(x => x.ContentTypeId == (int) basic.Id).ToArray();
                foreach (var blueprint in docTypeBluePrints)
                {
                    basic.Blueprints[blueprint.Id] = blueprint.Name;
                }
            }

            return basics.OrderBy(c => contentId == Constants.System.Root ? c.Name : string.Empty);
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
