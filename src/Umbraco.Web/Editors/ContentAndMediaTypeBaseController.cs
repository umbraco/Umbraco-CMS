using System.Net;
using System.Web.Http;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Newtonsoft.Json;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Am abstract API controller providing functionality used for dealing with content and media types
    /// </summary>
    [PluginController("UmbracoApi")]    
    public abstract class ContentAndMediaTypeBaseController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentAndMediaTypeBaseController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentAndMediaTypeBaseController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns the container configuration JSON structure for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public ContentTypeContainerConfiguration GetContainerConfig(int contentId)
        {
            var contentItem = Services.ContentService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!string.IsNullOrEmpty(contentItem.ContentType.ContainerConfig))
            {
                return JsonConvert.DeserializeObject<ContentTypeContainerConfiguration>(contentItem.ContentType.ContainerConfig);
            }

            return null;
        }
    }
}