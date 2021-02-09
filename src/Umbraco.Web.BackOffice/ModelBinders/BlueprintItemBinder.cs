using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.BackOffice.ModelBinders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        private readonly IContentService _contentService;

        public BlueprintItemBinder(IJsonSerializer jsonSerializer, UmbracoMapper umbracoMapper, IContentService contentService, IContentTypeService contentTypeService, IHostingEnvironment hostingEnvironment) : base(jsonSerializer, umbracoMapper, contentService, contentTypeService, hostingEnvironment)
        {
            _contentService = contentService;
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return _contentService.GetBlueprintById(model.Id);
        }
    }
}
