using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Binders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        private readonly IContentService _contentService;
        public BlueprintItemBinder(IContentService contentService)
            :base(contentService)
        {
            _contentService = contentService;
        }

        public BlueprintItemBinder(ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService)
            : base(logger, services, umbracoContextAccessor, contentService)
        {
            _contentService = contentService;
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return _contentService.GetBlueprintById(model.Id);
        }
    }
}
