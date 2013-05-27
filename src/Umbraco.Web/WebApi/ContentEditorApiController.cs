using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{
    [PluginController("UmbracoEditors")]
    [ValidationFilter]
    public class ContentEditorApiController : UmbracoApiController
    {
        private readonly ContentModelMapper _contentModelMapper;

        public ContentEditorApiController()
            : this(UmbracoContext.Current, new ContentModelMapper(UmbracoContext.Current.Application))
        {            
        }

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
        public HttpResponseMessage PostSaveContent(
            [ModelBinder(typeof(ContentItemBinder))]
            ContentItemSave contentItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use

            return Request.CreateResponse(HttpStatusCode.OK, "success!");
        }

    }
}
