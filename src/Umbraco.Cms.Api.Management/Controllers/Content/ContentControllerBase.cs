﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Content;

public class ContentControllerBase : ManagementApiControllerBase
{
    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentEditingOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the content operation.")
                .Build()),
            ContentEditingOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested content could not be found")
                .Build()),
            ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch => BadRequest(problemDetailsBuilder
                .WithTitle("Content type culture variance mismatch")
                .WithDetail("The content type variance did not match that of the passed content data.")
                .Build()),
            ContentEditingOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The content could not be found")
                .Build()),
            ContentEditingOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The targeted content parent could not be found")
                .Build()),
            ContentEditingOperationStatus.ParentInvalid => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid parent")
                .WithDetail("The targeted parent was not valid for the operation.")
                .Build()),
            ContentEditingOperationStatus.NotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("Operation not permitted")
                .WithDetail(
                    "The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                .Build()),
            ContentEditingOperationStatus.TemplateNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The template could not be found")
                .Build()),
            ContentEditingOperationStatus.TemplateNotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("Template not allowed")
                .WithDetail("The selected template was not allowed for the operation.")
                .Build()),
            ContentEditingOperationStatus.PropertyTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("One or more property types could not be found")
                .Build()),
            ContentEditingOperationStatus.InTrash => BadRequest(problemDetailsBuilder
                .WithTitle("Content is in the recycle bin")
                .WithDetail("Could not perform the operation because the targeted content was in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.NotInTrash => BadRequest(problemDetailsBuilder
                .WithTitle("Content is not in the recycle bin")
                .WithDetail("The attempted operation requires the targeted content to be in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.SortingInvalid => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid sorting options")
                .WithDetail("The supplied sorting operations were invalid. Additional details can be found in the log.")
                .Build()),
            ContentEditingOperationStatus.InvalidCulture => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid culture")
                .WithDetail("One or more of the supplied culture codes did not match the configured languages.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(
                StatusCodes.Status500InternalServerError,
                problemDetailsBuilder
                    .WithTitle("Unknown error. Please see the log for more details.")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content operation status.")
                .Build()),
        });

    protected IActionResult ContentEditingOperationStatusResult<TContentModelBase, TValueModel, TVariantModel>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<TValueModel, TVariantModel>
        where TValueModel : ValueModelBase
        where TVariantModel : VariantModelBase
    {
        if (status is not ContentEditingOperationStatus.PropertyValidationError)
        {
            return ContentEditingOperationStatusResult(status);
        }

        var errors = new SortedDictionary<string, string[]>();
        var missingPropertyAliases = new List<string>();
        foreach (PropertyValidationError validationError in validationResult.ValidationErrors)
        {
            TValueModel? requestValue = requestModel.Values.FirstOrDefault(value =>
                value.Alias == validationError.Alias
                && value.Culture == validationError.Culture
                && value.Segment == validationError.Segment);
            if (requestValue is null)
            {
                missingPropertyAliases.Add(validationError.Alias);
                continue;
            }

            var index = requestModel.Values.IndexOf(requestValue);
            var key = $"$.{nameof(ContentModelBase<TValueModel, TVariantModel>.Values).ToFirstLowerInvariant()}[{index}].{nameof(ValueModelBase.Value).ToFirstLowerInvariant()}{validationError.JsonPath}";
            errors.Add(key, validationError.ErrorMessages);
        }

        return OperationStatusResult(status, problemDetailsBuilder
            => BadRequest(problemDetailsBuilder
                .WithTitle("Validation failed")
                .WithDetail("One or more properties did not pass validation")
                .WithRequestModelErrors(errors)
                .WithExtension("missingProperties", missingPropertyAliases.ToArray())
                .Build()));
    }
}
