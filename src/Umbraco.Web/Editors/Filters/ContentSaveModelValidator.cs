using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validation helper for <see cref="ContentItemSave"/>
    /// </summary>
    internal class ContentSaveModelValidator : ContentModelValidator<IContent, ContentItemSave, ContentVariantSave>
    {
        public ContentSaveModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, umbracoContextAccessor)
        {
        }
    }
}
