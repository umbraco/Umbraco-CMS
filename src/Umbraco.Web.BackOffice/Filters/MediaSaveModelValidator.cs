using Microsoft.Extensions.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Validator for <see cref="MediaItemSave"/>
    /// </summary>
    internal class MediaSaveModelValidator : ContentModelValidator<IMedia, MediaItemSave, IContentProperties<ContentPropertyBasic>>
    {
        public MediaSaveModelValidator(
            ILogger<MediaSaveModelValidator> logger,
            IBackOfficeSecurity backofficeSecurity,
            ILocalizedTextService textService,
            IPropertyValidationService propertyValidationService)
            : base(logger, backofficeSecurity, textService, propertyValidationService)
        {
        }
    }
}
