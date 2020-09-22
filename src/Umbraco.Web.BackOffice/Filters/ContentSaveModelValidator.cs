﻿using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Validator for <see cref="ContentItemSave"/>
    /// </summary>
    internal class ContentSaveModelValidator : ContentModelValidator<IContent, ContentItemSave, ContentVariantSave>
    {
        public ContentSaveModelValidator(
            ILogger logger,
            IBackofficeSecurity backofficeSecurity,
            ILocalizedTextService textService,
            IPropertyValidationService propertyValidationService)
            : base(logger, backofficeSecurity, textService, propertyValidationService)
        {
        }

    }
}
