using System.Linq;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;

namespace Segments.Features.Segments
{
    public class SegmentsApiController : UmbracoApiController
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;

        public SegmentsApiController(
            IScopeProvider scopeProvider,
            IContentService contentService,
            IContentTypeService contentTypeService)
        {
            _scopeProvider = scopeProvider;
            _contentService = contentService;
            _contentTypeService = contentTypeService;
        }

        [HttpGet]
        public void Setup()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var homepage = _contentService.GetByLevel(1)?.FirstOrDefault();
                if (homepage == null) return;

                var contentType = _contentTypeService.Get(homepage.ContentTypeId);

                // Add Segment variation type
                contentType.Variations |= ContentVariation.Segment;

                foreach (var propertyType in contentType.PropertyTypes)
                {
                    // Add Segment variation to property
                    propertyType.Variations |= ContentVariation.Segment;
                }

                // _contentTypeService.Save(contentType);

                homepage.SetValue("title", "Variant Title", culture: "en-US", segment: "b");

                _contentService.SaveAndPublish(homepage);

                scope.Complete();
            }
        }
    }
}
