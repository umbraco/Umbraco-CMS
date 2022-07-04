using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     Provides a base class for authorized auto-routed Umbraco API controllers.
/// </summary>
/// <remarks>
///     This controller will also append a custom header to the response if the user
///     is logged in using forms authentication which indicates the seconds remaining
///     before their timeout expires.
/// </remarks>
[AngularJsonOnlyConfiguration] // TODO: This could be applied with our Application Model conventions
[JsonExceptionFilter]
[IsBackOffice]
[UmbracoUserTimeoutFilter]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[DisableBrowserCache]
[UmbracoRequireHttps]
[CheckIfUserTicketDataIsStale]
[MiddlewareFilter(typeof(UnhandledExceptionLoggerFilter))]
public abstract class UmbracoAuthorizedApiController : UmbracoApiController
{
    /// <summary>
    ///     Returns a validation problem result for the <see cref="IErrorModel" /> and the <see cref="ModelStateDictionary" />
    /// </summary>
    /// <param name="model"></param>
    /// <param name="modelStateDictionary"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected virtual ActionResult ValidationProblem(IErrorModel? model, ModelStateDictionary modelStateDictionary,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        if (model is not null)
        {
            model.Errors = modelStateDictionary.ToErrorDictionary();
        }

        return ValidationProblem(model, statusCode);
    }

    /// <summary>
    ///     Overridden to return Umbraco compatible errors
    /// </summary>
    /// <param name="modelStateDictionary"></param>
    /// <returns></returns>
    [NonAction]
    public override ActionResult ValidationProblem(ModelStateDictionary modelStateDictionary) =>
        new ValidationErrorResult(new SimpleValidationModel(modelStateDictionary.ToErrorDictionary()));

    //ValidationProblemDetails problemDetails = GetValidationProblemDetails(modelStateDictionary: modelStateDictionary);
    //return new ValidationErrorResult(problemDetails);
    // creates validation problem details instance.
    // borrowed from netcore: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ControllerBase.cs#L1970
    protected ValidationProblemDetails? GetValidationProblemDetails(
        string? detail = null,
        string? instance = null,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        [ActionResultObjectValue] ModelStateDictionary? modelStateDictionary = null)
    {
        modelStateDictionary ??= ModelState;

        ValidationProblemDetails? validationProblem;
        if (ProblemDetailsFactory == null)
        {
            // ProblemDetailsFactory may be null in unit testing scenarios. Improvise to make this more testable.
            validationProblem = new ValidationProblemDetails(modelStateDictionary)
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type
            };
        }
        else
        {
            validationProblem = ProblemDetailsFactory?.CreateValidationProblemDetails(
                HttpContext,
                modelStateDictionary,
                statusCode,
                title,
                type,
                detail,
                instance);
        }

        return validationProblem;
    }

    /// <summary>
    ///     Returns an Umbraco compatible validation problem for the given error message
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    protected virtual ActionResult ValidationProblem(string errorMessage)
    {
        ValidationProblemDetails? problemDetails = GetValidationProblemDetails(errorMessage);
        return new ValidationErrorResult(problemDetails);
    }

    /// <summary>
    ///     Returns an Umbraco compatible validation problem for the object result
    /// </summary>
    /// <param name="value"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected virtual ActionResult ValidationProblem(object? value, int statusCode)
        => new ValidationErrorResult(value, statusCode);

    /// <summary>
    ///     Returns an Umbraco compatible validation problem for the given notification model
    /// </summary>
    /// <param name="model"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected virtual ActionResult ValidationProblem(INotificationModel? model,
        int statusCode = StatusCodes.Status400BadRequest)
        => new ValidationErrorResult(model, statusCode);
}
