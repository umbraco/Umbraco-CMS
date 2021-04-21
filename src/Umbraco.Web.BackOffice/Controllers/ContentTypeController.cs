using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;
using ContentType = Umbraco.Cms.Core.Models.ContentType;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class ContentTypeController : ContentTypeControllerBase<IContentType>
    {
        // TODO: Split this controller apart so that authz is consistent, currently we need to authz each action individually.
        // It would be possible to have something like a ContentTypeInfoController for the GetAllPropertyTypeAliases/GetCount/GetAllowedChildren/etc... actions

        private readonly IEntityXmlSerializer _serializer;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IContentTypeService _contentTypeService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IDataTypeService _dataTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IFileService _fileService;
        private readonly ILogger<ContentTypeController> _logger;
        private readonly IContentService _contentService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly PackageDataInstallation _packageDataInstallation;

        public ContentTypeController(
            ICultureDictionary cultureDictionary,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMemberTypeService memberTypeService,
            IUmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            IEntityXmlSerializer serializer,
            PropertyEditorCollection propertyEditors,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IFileService fileService,
            ILogger<ContentTypeController> logger,
            IContentService contentService,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IHostingEnvironment hostingEnvironment,
            EditorValidatorCollection editorValidatorCollection,
            PackageDataInstallation packageDataInstallation)
            : base(cultureDictionary,
                editorValidatorCollection,
                contentTypeService,
                mediaTypeService,
                memberTypeService,
                umbracoMapper,
                localizedTextService)
        {
            _serializer = serializer;
            _propertyEditors = propertyEditors;
            _contentTypeService = contentTypeService;
            _umbracoMapper = umbracoMapper;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _dataTypeService = dataTypeService;
            _shortStringHelper = shortStringHelper;
            _localizedTextService = localizedTextService;
            _fileService = fileService;
            _logger = logger;
            _contentService = contentService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _hostingEnvironment = hostingEnvironment;
            _packageDataInstallation = packageDataInstallation;
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public int GetCount()
        {
            return _contentTypeService.Count();
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public bool HasContentNodes(int id)
        {
            return _contentTypeService.HasContentNodes(id);
        }

        /// <summary>
        /// Gets the document type a given id
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<DocumentTypeDisplay> GetById(int id)
        {
            var ct = _contentTypeService.Get(id);
            if (ct == null)
            {
                return NotFound();
            }

            var dto = _umbracoMapper.Map<IContentType, DocumentTypeDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Gets the document type a given guid
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<DocumentTypeDisplay> GetById(Guid id)
        {
            var contentType = _contentTypeService.Get(id);
            if (contentType == null)
            {
                return NotFound();
            }

            var dto = _umbracoMapper.Map<IContentType, DocumentTypeDisplay>(contentType);
            return dto;
        }

        /// <summary>
        /// Gets the document type a given udi
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<DocumentTypeDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                return NotFound();

            var contentType = _contentTypeService.Get(guidUdi.Guid);
            if (contentType == null)
            {
                return NotFound();
            }

            var dto = _umbracoMapper.Map<IContentType, DocumentTypeDisplay>(contentType);
            return dto;
        }

        /// <summary>
        /// Deletes a document type with a given ID
        /// </summary>
        [HttpDelete]
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult DeleteById(int id)
        {
            var foundType = _contentTypeService.Get(id);
            if (foundType == null)
            {
                return NotFound();
            }

            _contentTypeService.Delete(foundType, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
            return Ok();
        }

        /// <summary>
        /// Gets all user defined properties.
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessAnyContentOrTypes)]
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            return _contentTypeService.GetAllPropertyTypeAliases();
        }

        /// <summary>
        /// Gets all the standard fields.
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessAnyContentOrTypes)]
        public IEnumerable<string> GetAllStandardFields()
        {
            string[] preValuesSource = { "createDate", "creatorName", "level", "nodeType", "nodeTypeAlias", "pageID", "pageName", "parentID", "path", "template", "updateDate", "writerID", "writerName" };
            return preValuesSource;
        }

        /// <summary>
        /// Returns the available compositions for this content type
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult GetAvailableCompositeContentTypes(GetAvailableCompositionsFilter filter)
        {
            var actionResult = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId,
                UmbracoObjectTypes.DocumentType, filter.FilterContentTypes, filter.FilterPropertyTypes,
                filter.IsElement);

            if (!(actionResult.Result is null))
            {
                return actionResult.Result;
            }

            var result = actionResult.Value
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Ok(result);
        }

        /// <summary>
        /// Returns true if any content types have culture variation enabled
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        public bool AllowsCultureVariation()
        {
            IEnumerable<IContentType> contentTypes = _contentTypeService.GetAll();
            return contentTypes.Any(contentType => contentType.VariesByCulture());
        }

        /// <summary>
        /// Returns where a particular composition has been used
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult GetWhereCompositionIsUsedInContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetWhereCompositionIsUsedInContentTypes(filter.ContentTypeId, UmbracoObjectTypes.DocumentType).Value
                .Select(x => new
                {
                    contentType = x
                });
            return Ok(result);
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessAnyContentOrTypes)]
        public ActionResult<ContentPropertyDisplay> GetPropertyTypeScaffold(int id)
        {
            var dataTypeDiff = _dataTypeService.GetDataType(id);

            if (dataTypeDiff == null)
            {
                return NotFound();
            }

            var configuration = _dataTypeService.GetDataType(id).Configuration;
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
        [HttpDelete]
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult DeleteContainer(int id)
        {
            _contentTypeService.DeleteContainer(id, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            return Ok();
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult PostCreateContainer(int parentId, string name)
        {
            var result = _contentTypeService.CreateContainer(parentId, name, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            if (result.Success)
                return Ok(result.Result); //return the id
            else
                return ValidationErrorResult.CreateNotificationValidationErrorResult(result.Exception.Message);
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult PostRenameContainer(int id, string name)
        {
            var result = _contentTypeService.RenameContainer(id, name, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

            if (result.Success)
                return Ok(result.Result); //return the id
            else
                return ValidationErrorResult.CreateNotificationValidationErrorResult(result.Exception.Message);
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<DocumentTypeDisplay> PostSave(DocumentTypeSave contentTypeSave)
        {
            //Before we send this model into this saving/mapping pipeline, we need to do some cleanup on variations.
            //If the doc type does not allow content variations, we need to update all of it's property types to not allow this either
            //else we may end up with ysods. I'm unsure if the service level handles this but we'll make sure it is updated here
            if (!contentTypeSave.AllowCultureVariant)
            {
                foreach(var prop in contentTypeSave.Groups.SelectMany(x => x.Properties))
                {
                    prop.AllowCultureVariant = false;
                }
            }

            var savedCt = PerformPostSave<DocumentTypeDisplay, DocumentTypeSave, PropertyTypeBasic>(
                contentTypeSave:    contentTypeSave,
                getContentType:     i => _contentTypeService.Get(i),
                saveContentType:    type => _contentTypeService.Save(type),
                beforeCreateNew:    ctSave =>
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

            if (!(savedCt.Result is null))
            {
                return savedCt.Result;
            }

            var display = _umbracoMapper.Map<DocumentTypeDisplay>(savedCt.Value);


            display.AddSuccessNotification(
                            _localizedTextService.Localize("speechBubbles/contentTypeSavedHeader"),
                            string.Empty);

            return display;
        }

        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<TemplateDisplay> PostCreateDefaultTemplate(int id)
        {
            var contentType = _contentTypeService.Get(id);
            if (contentType == null)
            {
                return NotFound("No content type found with id " + id);
            }

            var template = CreateTemplateForContentType(contentType.Alias, contentType.Name);
            if (template == null)
            {
                throw new InvalidOperationException("Could not create default template for content type with id " + id);
            }

            return _umbracoMapper.Map<TemplateDisplay>(template);
        }

        private ITemplate CreateTemplateForContentType(string contentTypeAlias, string contentTypeName)
        {
            var template = _fileService.GetTemplate(contentTypeAlias);
            if (template == null)
            {
                var tryCreateTemplate = _fileService.CreateTemplateForContentType(contentTypeAlias, contentTypeName);
                if (tryCreateTemplate == false)
                {
                    _logger.LogWarning("Could not create a template for Content Type: \"{ContentTypeAlias}\", status: {Status}",
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
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public DocumentTypeDisplay GetEmpty(int parentId)
        {
            IContentType ct;
            if (parentId != Constants.System.Root)
            {
                var parent = _contentTypeService.Get(parentId);
                ct = parent != null ? new ContentType(_shortStringHelper, parent, string.Empty) : new ContentType(_shortStringHelper, parentId);
            }
            else
                ct = new ContentType(_shortStringHelper, parentId);

            ct.Icon = Constants.Icons.Content;

            var dto = _umbracoMapper.Map<IContentType, DocumentTypeDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all content type objects
        /// </summary>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IEnumerable<ContentTypeBasic> GetAll()
        {
            var types = _contentTypeService.GetAll();
            var basics = types.Select(_umbracoMapper.Map<IContentType, ContentTypeBasic>);

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
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IContentType> types;
            if (contentId == Constants.System.Root)
            {
                types = _contentTypeService.GetAll().Where(x => x.AllowedAsRoot).ToList();
            }
            else
            {
                var contentItem = _contentService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(contentItem);
                var ids = contentType.AllowedContentTypes.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = _contentTypeService.GetAll(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
            }

            var basics = types.Where(type => type.IsElement == false).Select(_umbracoMapper.Map<IContentType, ContentTypeBasic>).ToList();

            var localizedTextService = _localizedTextService;
            foreach (var basic in basics)
            {
                basic.Name = localizedTextService.UmbracoDictionaryTranslate(CultureDictionary, basic.Name);
                basic.Description = localizedTextService.UmbracoDictionaryTranslate(CultureDictionary, basic.Description);
            }

            //map the blueprints
            var blueprints = _contentService.GetBlueprintsForContentTypes(types.Select(x => x.Id).ToArray()).ToArray();
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
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move,
                getContentType: i => _contentTypeService.Get(i),
                doMove: (type, i) => _contentTypeService.Move(type, i));
        }

        /// <summary>
        /// Copy the content type
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult PostCopy(MoveOrCopy copy)
        {
            return PerformCopy(
                copy,
                getContentType: i => _contentTypeService.Get(i),
                doCopy: (type, i) => _contentTypeService.Copy(type, i));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult Export(int id)
        {
            var contentType = _contentTypeService.Get(id);
            if (contentType == null) throw new NullReferenceException("No content type found with id " + id);

            var xml = _serializer.Serialize(contentType);

            var fileName = $"{contentType.Alias}.udt";
            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            HttpContext.Response.Headers.Add("x-filename", fileName);

            return File( Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet, fileName);

        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public IActionResult Import(string file)
        {
            var filePath = Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data), file);
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }


            var xd = new XmlDocument {XmlResolver = null};
            xd.Load(filePath);

            var userId = _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0);
            var element = XElement.Parse(xd.InnerXml);
            _packageDataInstallation.ImportDocumentType(element, userId);

            // Try to clean up the temporary file.
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temporary udt file in {File}", filePath);
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
        public ActionResult<ContentTypeImportModel> Upload(List<IFormFile> file)
        {
            var model = new ContentTypeImportModel();

            foreach (var formFile in file)
            {
                var fileName = formFile.FileName.Trim(Constants.CharArrays.DoubleQuote);
                var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

                var root = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
                var tempPath = Path.Combine(root,fileName);

                using (var stream = System.IO.File.Create(tempPath))
                {
                    formFile.CopyToAsync(stream).GetAwaiter().GetResult();
                }

                if (ext.InvariantEquals("udt"))
                {
                    model.TempFileName = Path.Combine(root, fileName);

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
                    model.Notifications.Add(new BackOfficeNotification(
                        _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                        _localizedTextService.Localize("media/disallowedFileType"),
                        NotificationStyle.Warning));
                }
            }



            return model;

        }


    }
}
