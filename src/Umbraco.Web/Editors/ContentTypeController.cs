using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;

namespace Umbraco.Web.Editors
{
    // TODO:  We'll need to be careful about the security on this controller, when we start implementing
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like
    //  GetAllowedChildren, GetPropertyTypeScaffold, GetAllPropertyTypeAliases are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
    [EnableOverrideAuthorization]
    [ContentTypeControllerConfiguration]
    public class ContentTypeController : ContentTypeControllerBase<IContentType>
    {
        private readonly IEntityXmlSerializer _serializer;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IScopeProvider _scopeProvider;

        public ContentTypeController(IEntityXmlSerializer serializer,
            ICultureDictionaryFactory cultureDictionaryFactory,
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, PropertyEditorCollection propertyEditors,
            ServiceContext services, AppCaches appCaches,
            IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper,
            IScopeProvider scopeProvider)
            : base(cultureDictionaryFactory, globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _serializer = serializer;
            _propertyEditors = propertyEditors;
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class ContentTypeControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))));            }
        }

        public int GetCount()
        {
            return Services.ContentTypeService.Count();
        }

        [HttpGet]
        [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
        public bool HasContentNodes(int id)
        {
            return Services.ContentTypeService.HasContentNodes(id);
        }

        /// <summary>
        /// Gets the document type a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentTypeDisplay GetById(int id)
        {
            var contentType = Services.ContentTypeService.Get(id);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(contentType);
            return dto;
        }

        /// <summary>
        /// Gets the document type a given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentTypeDisplay GetById(Guid id)
        {
            var contentType = Services.ContentTypeService.Get(id);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(contentType);
            return dto;
        }

        /// <summary>
        /// Gets the document type a given udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var contentType = Services.ContentTypeService.Get(guidUdi.Guid);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(contentType);
            return dto;
        }

        /// <summary>
        /// Deletes a document type with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.ContentTypeService.Get(id);
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
            return Services.ContentTypeService.GetAllPropertyTypeAliases();
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
        /// Returns the available compositions for this content type
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetAvailableCompositeContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId, UmbracoObjectTypes.DocumentType, filter.FilterContentTypes, filter.FilterPropertyTypes, filter.IsElement)
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
            var dataTypeDiff = Services.DataTypeService.GetDataType(id);

            if (dataTypeDiff == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var configuration = Services.DataTypeService.GetDataType(id).Configuration;
            var editor = _propertyEditors[dataTypeDiff.EditorAlias];

            return new ContentPropertyDisplay()
            {
                Editor = dataTypeDiff.EditorAlias,
                Validation = new PropertyTypeValidation(),
                View = editor.GetValueEditor().View,
                Config = editor.GetConfigurationEditor().ToConfigurationEditor(configuration)
            };
        }

        /// <summary>
        /// Deletes a document type container with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteContainer(int id)
        {
            Services.ContentTypeService.DeleteContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.ContentTypeService.CreateContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public HttpResponseMessage PostRenameContainer(int id, string name)
        {
            var result = Services.ContentTypeService.RenameContainer(id, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public DocumentTypeDisplay PostSave(DocumentTypeSave contentTypeSave)
        {
            //Before we send this model into this saving/mapping pipeline, we need to do some cleanup on variations.
            //If the doc type does not allow content variations, we need to update all of it's property types to not allow this either
            //else we may end up with ysods. I'm unsure if the service level handles this but we'll make sure it is updated here
            if (!contentTypeSave.AllowCultureVariant)
            {
                foreach (var prop in contentTypeSave.Groups.SelectMany(x => x.Properties))
                {
                    prop.AllowCultureVariant = false;
                }
            }

            var savedCt = PerformPostSave<DocumentTypeDisplay, DocumentTypeSave, PropertyTypeBasic>(
                contentTypeSave: contentTypeSave,
                getContentType: i => Services.ContentTypeService.Get(i),
                saveContentType: type => Services.ContentTypeService.Save(type),
                beforeCreateNew: ctSave =>
                {
                    //create a default template if it doesn't exist -but only if default template is == to the content type
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
                            Services.TextService.Localize("speechBubbles", "contentTypeSavedHeader"),
                            string.Empty);

            return display;
        }

        public TemplateDisplay PostCreateDefaultTemplate(int id)
        {
            var contentType = Services.ContentTypeService.Get(id);
            if (contentType == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, "No content type found with id " + id));
            }

            var template = CreateTemplateForContentType(contentType.Alias, contentType.Name);
            if (template == null)
            {
                throw new InvalidOperationException("Could not create default template for content type with id " + id);
            }

            return Mapper.Map<TemplateDisplay>(template);
        }

        private ITemplate CreateTemplateForContentType(string contentTypeAlias, string contentTypeName)
        {
            var template = Services.FileService.GetTemplate(contentTypeAlias);
            if (template == null)
            {
                var tryCreateTemplate = Services.FileService.CreateTemplateForContentType(contentTypeAlias, contentTypeName);
                if (tryCreateTemplate == false)
                {
                    Logger.Warn<ContentTypeController, string, OperationResultType>("Could not create a template for Content Type: \"{ContentTypeAlias}\", status: {Status}",
                        contentTypeAlias, tryCreateTemplate.Result.Result);
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
                var parent = Services.ContentTypeService.Get(parentId);
                ct = parent != null ? new ContentType(parent, string.Empty) : new ContentType(parentId);
            }
            else
                ct = new ContentType(parentId);

            ct.Icon = Constants.Icons.Content;

            var dto = Mapper.Map<IContentType, DocumentTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all content type objects
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {
            var types = Services.ContentTypeService.GetAll();
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
                types = Services.ContentTypeService.GetAll().Where(x => x.AllowedAsRoot).ToList();
            }
            else
            {
                var contentItem = Services.ContentService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var contentType = Services.ContentTypeBaseServices.GetContentTypeOf(contentItem);
                var ids = contentType.AllowedContentTypes.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAll(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
            }

            var basics = types.Where(type => type.IsElement == false).Select(Mapper.Map<IContentType, ContentTypeBasic>).ToList();

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
                getContentType: i => Services.ContentTypeService.Get(i),
                doMove: (type, i) => Services.ContentTypeService.Move(type, i));
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
                getContentType: i => Services.ContentTypeService.Get(i),
                doCopy: (type, i) => Services.ContentTypeService.Copy(type, i));
        }

        [HttpGet]
        public HttpResponseMessage Export(int id)
        {
            var contentType = Services.ContentTypeService.Get(id);
            if (contentType == null) throw new NullReferenceException("No content type found with id " + id);

            var xml = _serializer.Serialize(contentType);

            var response = new HttpResponseMessage
            {
                Content = new StringContent(xml.ToDataString())
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = $"{contentType.Alias}.udt"
                        },
                        ContentType =   new MediaTypeHeaderValue( "application/octet-stream")

                    }
                }
            };

            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            response.Headers.Add("x-filename", $"{contentType.Alias}.udt");

            return response;
        }

        [HttpPost]
        public HttpResponseMessage Import(string file)
        {
            var filePath = Path.Combine(IOHelper.MapPath(SystemDirectories.Data), file);
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(filePath))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var dataInstaller = new PackageDataInstallation(Logger, Services.FileService, Services.MacroService, Services.LocalizationService,
                Services.DataTypeService, Services.EntityService, Services.ContentTypeService, Services.ContentService, _propertyEditors, _scopeProvider);

            var xd = new XmlDocument {XmlResolver = null};
            xd.Load(filePath);

            var userId = Security.GetUserId().ResultOr(0);
            var element = XElement.Parse(xd.InnerXml);
            dataInstaller.ImportDocumentType(element, userId);

            // Try to clean up the temporary file.
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error<ContentTypeController, string>(ex, "Error cleaning up temporary udt file in App_Data: {File}", filePath);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<ContentTypeImportModel> Upload()
        {
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath(SystemDirectories.TempData.EnsureEndsWith('/') + "FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);
            var result = await Request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var model = new ContentTypeImportModel();

            var file = result.FileData[0];
            var fileName = file.Headers.ContentDisposition.FileName.Trim(Constants.CharArrays.DoubleQuote);
            var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

            var destFileName = Path.Combine(root, fileName);
            if (Path.GetFullPath(destFileName).StartsWith(Path.GetFullPath(root)))
            {
                try
                {
                    // due to a bug before 8.7.0 we didn't delete temp files, so we need to make sure to delete before
                    // moving else you get errors and the upload fails without a message in the UI (there's a JS error)
                    if(System.IO.File.Exists(destFileName))
                        System.IO.File.Delete(destFileName);

                    // renaming the file because MultipartFormDataStreamProvider has created a random fileName instead of using the name from the
                    // content-disposition for more than 6 years now. Creating a CustomMultipartDataStreamProvider deriving from MultipartFormDataStreamProvider
                    // seems like a cleaner option, but I'm not sure where to put it and renaming only takes one line of code.
                    System.IO.File.Move(result.FileData[0].LocalFileName, destFileName);
                }
                catch (Exception ex)
                {
                    Logger.Error<ContentTypeController, string>(ex, "Error uploading udt file to App_Data: {File}", destFileName);
                }

                if (ext.InvariantEquals("udt"))
                {
                    model.TempFileName = destFileName;

                    var xd = new XmlDocument
                    {
                        XmlResolver = null
                    };
                    xd.Load(model.TempFileName);

                    model.Alias = xd.DocumentElement?.SelectSingleNode("//DocumentType/Info/Alias")?.FirstChild.Value;
                    model.Name = xd.DocumentElement?.SelectSingleNode("//DocumentType/Info/Name")?.FirstChild.Value;
                }
                else
                {
                    // Cleanup the temp file
                    System.IO.File.Delete(destFileName);
                    model.Notifications.Add(new Notification(
                        Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                        Services.TextService.Localize("media", "disallowedFileType"),
                        NotificationStyle.Warning));
                }
            }
            else
            {
                // Cleanup the temp file
                System.IO.File.Delete(result.FileData[0].LocalFileName);
                model.Notifications.Add(new Notification(
                                        Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                                        Services.TextService.Localize("media", "invalidFileName"),
                                        NotificationStyle.Warning));
            }

            return model;

        }


    }
}
