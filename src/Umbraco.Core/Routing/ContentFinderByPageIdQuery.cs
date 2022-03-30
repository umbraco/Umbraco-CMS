using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// This looks up a document by checking for the umbPageId of a request/query string
    /// </summary>
    /// <remarks>
    /// This is used by library.RenderTemplate and also some of the macro rendering functionality like in
    /// macroResultWrapper.aspx
    /// </remarks>
    public class ContentFinderByPageIdQuery : IContentFinder
    {
        private readonly IRequestAccessor _requestAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByPageIdQuery"/> class.
        /// </summary>
        public ContentFinderByPageIdQuery(IRequestAccessor requestAccessor, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _requestAccessor = requestAccessor ?? throw new System.ArgumentNullException(nameof(requestAccessor));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new System.ArgumentNullException(nameof(umbracoContextAccessor));
        }

        /// <inheritdoc/>
        public bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            if(!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return false;
            }
            if (int.TryParse(_requestAccessor.GetRequestValue("umbPageID"),  NumberStyles.Integer, CultureInfo.InvariantCulture, out int pageId))
            {
                IPublishedContent? doc = umbracoContext.Content?.GetById(pageId);

                if (doc != null)
                {
                    frequest.SetPublishedContent(doc);
                    return true;
                }
            }

            return false;
        }
    }
}
