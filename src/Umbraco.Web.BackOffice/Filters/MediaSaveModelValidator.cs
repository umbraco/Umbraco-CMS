using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Validator for <see cref="MediaItemSave" />
/// </summary>
internal class
    MediaSaveModelValidator : ContentModelValidator<IMedia, MediaItemSave, IContentProperties<ContentPropertyBasic>>
{
    public MediaSaveModelValidator(
        ILogger<MediaSaveModelValidator> logger,
        IPropertyValidationService propertyValidationService)
        : base(logger, propertyValidationService)
    {
    }
}
