﻿using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

internal interface IContentValidationServiceBase<in TContentType>
    where TContentType : IContentTypeComposition
{
    Task<ContentValidationResult> ValidatePropertiesAsync(ContentEditingModelBase contentEditingModelBase, TContentType contentType);

    Task<bool> ValidateCulturesAsync(ContentEditingModelBase contentEditingModelBase);
}
