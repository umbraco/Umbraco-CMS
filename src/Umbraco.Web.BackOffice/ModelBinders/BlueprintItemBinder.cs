using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Binders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        private readonly ContentService _contentService;

        public BlueprintItemBinder(IJsonSerializer jsonSerializer, UmbracoMapper umbracoMapper, IContentService contentService, IContentTypeService contentTypeService, IHostingEnvironment hostingEnvironment, ContentService contentService2) : base(jsonSerializer, umbracoMapper, contentService, contentTypeService, hostingEnvironment)
        {
            _contentService = contentService2;
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return _contentService.GetBlueprintById(model.Id);
        }
    }
}
