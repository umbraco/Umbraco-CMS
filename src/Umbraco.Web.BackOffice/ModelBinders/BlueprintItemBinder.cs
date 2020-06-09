using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Binders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        private readonly ContentService _contentService;

        public BlueprintItemBinder(UmbracoMapper umbracoMapper, ContentTypeService contentTypeService, ContentService contentService) : base(umbracoMapper, contentTypeService, contentService)
        {
            _contentService = contentService;
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return _contentService.GetBlueprintById(model.Id);
        }
    }
}
