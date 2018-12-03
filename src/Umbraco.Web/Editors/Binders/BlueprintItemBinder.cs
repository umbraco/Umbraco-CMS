using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Binders
{
    internal class BlueprintItemBinder : ContentItemBinder
    {
        public BlueprintItemBinder()
        {
        }

        public BlueprintItemBinder(ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, services, umbracoContextAccessor)
        {
        }

        protected override IContent GetExisting(ContentItemSave model)
        {
            return Services.ContentService.GetBlueprintById(model.Id);
        }
    }
}
