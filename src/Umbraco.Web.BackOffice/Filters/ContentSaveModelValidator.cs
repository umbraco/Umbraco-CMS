using Microsoft.Extensions.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Validator for <see cref="ContentItemSave"/>
    /// </summary>
    internal class ContentSaveModelValidator : ContentModelValidator<IContent, ContentItemSave, ContentVariantSave>
    {
        public ContentSaveModelValidator(
            ILogger<ContentSaveModelValidator> logger,
            IPropertyValidationService propertyValidationService)
            : base(logger, propertyValidationService)
        {
        }

    }
}
