using System.ComponentModel.DataAnnotations;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validator for <see cref="ContentItemSave"/>
    /// </summary>
    internal class ContentSaveModelValidator : ContentModelValidator<IContent, ContentItemSave, ContentVariantSave>
    {
        public ContentSaveModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService textService) : base(logger, umbracoContextAccessor, textService)
        {
        }

    }
}
