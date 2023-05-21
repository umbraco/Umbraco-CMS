using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ActionsResults;

// TODO: This should probably follow the same conventions as in aspnet core and use ProblemDetails
// and ProblemDetails factory. See https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ControllerBase.cs#L1977
// ProblemDetails is explicitly checked for in the application model.
// In our base class UmbracoAuthorizedApiController the logic is there to create a ProblemDetails.
// However, to do this will require changing how angular deals with errors since the response will
// probably be different. Would just be better to follow the aspnet patterns.

/// <summary>
///     Custom result to return a validation error message with required headers
/// </summary>
/// <remarks>
///     The default status code is a 400 http response
/// </remarks>
public class ValidationErrorResult : ObjectResult
{
    public ValidationErrorResult(ModelStateDictionary modelState)
        : this(new SimpleValidationModel(modelState.ToErrorDictionary()))
    {
    }

    public ValidationErrorResult(object? value, int statusCode)
        : base(value) => StatusCode = statusCode;

    public ValidationErrorResult(object? value)
        : this(value, StatusCodes.Status400BadRequest)
    {
    }

    // TODO: Like here, shouldn't we use ProblemDetails?
    public ValidationErrorResult(string errorMessage, int statusCode)
        : base(new { Message = errorMessage }) =>
        StatusCode = statusCode;

    public ValidationErrorResult(string errorMessage)
        : this(errorMessage, StatusCodes.Status400BadRequest)
    {
    }

    /// <summary>
    ///     Typically this should not be used and just use the ValidationProblem method on the base controller class.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static ValidationErrorResult CreateNotificationValidationErrorResult(string errorMessage)
    {
        var notificationModel = new SimpleNotificationModel { Message = errorMessage };
        notificationModel.AddErrorNotification(errorMessage, string.Empty);
        return new ValidationErrorResult(notificationModel);
    }

    public override void OnFormatting(ActionContext context)
    {
        base.OnFormatting(context);
        context.HttpContext.Response.Headers["X-Status-Reason"] = "Validation failed";
    }
}
