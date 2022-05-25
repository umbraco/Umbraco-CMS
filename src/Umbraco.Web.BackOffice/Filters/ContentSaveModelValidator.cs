using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Validator for <see cref="ContentItemSave" />
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
