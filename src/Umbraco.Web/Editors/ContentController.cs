using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ContentController : UmbracoAuthorizedJsonController
    {
        private readonly ContentModelMapper _contentModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentController()
            : this(UmbracoContext.Current, new ContentModelMapper(UmbracoContext.Current.Application, new UserModelMapper()))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="contentModelMapper"></param>
        internal ContentController(UmbracoContext umbracoContext, ContentModelMapper contentModelMapper)
            : base(umbracoContext)
        {
            _contentModelMapper = contentModelMapper;
        }

        public IEnumerable<ContentItemDisplay> GetByIds([FromUri]int[] ids)
        {
            var foundContent = ((ContentService) Services.ContentService).GetByIds(ids);

            return foundContent.Select(x => _contentModelMapper.ToContentItemDisplay(x));
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ContentItemDisplay GetById(int id)
        {
            var foundContent = Services.ContentService.GetById(id);
            if (foundContent == null)
            {
                ModelState.AddModelError("id", string.Format("content with id: {0} was not found", id));
                var errorResponse = Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    ModelState);
                throw new HttpResponseException(errorResponse);
            }
            return _contentModelMapper.ToContentItemDisplay(foundContent);
        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ContentItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = new Content("Empty", parentId, contentType);
            return _contentModelMapper.ToContentItemDisplay(emptyContent);
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        public ContentItemDisplay PostSave(
            [ModelBinder(typeof(ContentItemBinder))]
                ContentItemSave<IContent> contentItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object

            //We need to manually check the validation results here because:
            // * We still need to save the entity even if there are validation value errors
            // * Depending on if the entity is new, and if there are non property validation errors (i.e. the name is null)
            //      then we cannot continue saving, we can only display errors
            // * If there are validation errors and they were attempting to publish, we can only save, NOT publish and display 
            //      a message indicating this
            // TODO: WE need to implement the above points!

            if (!ModelState.IsValid)
            {
                if (ModelState["Name"] != null && ModelState["Name"].Errors.Any()
                    && (contentItem.Action == ContentSaveAction.SaveNew || contentItem.Action == ContentSaveAction.PublishNew))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, ModelState));
                }

                //if the model state is not valid we cannot publish so change it to save
                switch (contentItem.Action)
                {
                    case ContentSaveAction.Publish:
                        contentItem.Action = ContentSaveAction.Save;
                        break;
                    case ContentSaveAction.PublishNew:
                        contentItem.Action = ContentSaveAction.SaveNew;
                        break;
                }
            }

            //Now, we just need to save the data

            //Don't update the name if it is empty
            if (!contentItem.Name.IsNullOrWhiteSpace())
            {
                contentItem.PersistedContent.Name = contentItem.Name;    
            }
            
            //TODO: We'll need to save the new template, publishat, etc... values here

            //Save the property values
            foreach (var p in contentItem.ContentDto.Properties)
            {
                //get the dbo property
                var dboProperty = contentItem.PersistedContent.Properties[p.Alias];

                //create the property data to send to the property editor
                var d = new Dictionary<string, object>();
                //add the files if any
                var files = contentItem.UploadedFiles.Where(x => x.PropertyId == p.Id).ToArray();
                if (files.Any())
                {
                    d.Add("files", files);
                }
                var data = new ContentPropertyData(p.Value, d);
                
                //get the deserialized value from the property editor
                if (p.PropertyEditor == null)
                {
                    LogHelper.Warn<ContentController>("No property editor found for property " + p.Alias);
                }
                else
                {
                    dboProperty.Value = p.PropertyEditor.ValueEditor.DeserializeValue(data, dboProperty.Value);                    
                }
            }
            
            if (contentItem.Action == ContentSaveAction.Save || contentItem.Action == ContentSaveAction.SaveNew)
            {
                //save the item
                Services.ContentService.Save(contentItem.PersistedContent);
            }
            else
            {
                //publish the item
                Services.ContentService.SaveAndPublish(contentItem.PersistedContent);
            }
            

            //return the updated model
            var display = _contentModelMapper.ToContentItemDisplay(contentItem.PersistedContent);
            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            if (!ModelState.IsValid)
            {
                display.Errors = ModelState.ToErrorDictionary();
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Forbidden, display));
            }

            return display;
        }

    }
}