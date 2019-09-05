using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validator for <see cref="MediaItemSave"/>
    /// </summary>
    internal class MediaSaveModelValidator : ContentModelValidator<IMedia, MediaItemSave, IContentProperties<ContentPropertyBasic>>
    {
        public MediaSaveModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, umbracoContextAccessor)
        {
        }
    }
}