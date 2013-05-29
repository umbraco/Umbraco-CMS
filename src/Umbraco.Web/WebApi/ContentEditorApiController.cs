using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{

    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController("UmbracoEditors")]
    [ValidationFilter]
    public class ContentEditorApiController : UmbracoApiController
    {
        private readonly ContentModelMapper _contentModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentEditorApiController()
            : this(UmbracoContext.Current, new ContentModelMapper(UmbracoContext.Current.Application))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="contentModelMapper"></param>
        internal ContentEditorApiController(UmbracoContext umbracoContext, ContentModelMapper contentModelMapper)
            : base(umbracoContext)
        {
            _contentModelMapper = contentModelMapper;
        }

        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ContentItemDisplay GetContent(int id)
        {
            var foundContent = ApplicationContext.Services.ContentService.GetById(id);
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
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [ContentItemValidationFilter]
        [FileUploadCleanupFilter]
        public ContentItemDisplay PostSaveContent(
            [ModelBinder(typeof(ContentItemBinder))]
            ContentItemSave contentItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object

            //Now, we just need to save the data

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
                dboProperty.Value = p.PropertyEditor.ValueEditor.DeserializeValue(data, dboProperty.Value);
            }
            
            //save the item
            ApplicationContext.Services.ContentService.Save(contentItem.PersistedContent);

            //return the updated model
            return _contentModelMapper.ToContentItemDisplay(contentItem.PersistedContent);
        }

    }
}
