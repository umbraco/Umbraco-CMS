using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ActionsResults
{
    /// <summary>
    /// Custom result to return a validation error message with required headers
    /// </summary>
    /// <remarks>
    /// The default status code is a 400 http response
    /// </remarks>
    public class ValidationErrorResult : ObjectResult
    {
        public static ValidationErrorResult CreateNotificationValidationErrorResult(string errorMessage)
        {
            var notificationModel = new SimpleNotificationModel
            {
                Message = errorMessage
            };
            notificationModel.AddErrorNotification(errorMessage, string.Empty);
            return new ValidationErrorResult(notificationModel);
        }

        public ValidationErrorResult(object value, int statusCode) : base(value)
        {
            StatusCode = statusCode;
        }

        public ValidationErrorResult(object value) : this(value, StatusCodes.Status400BadRequest)
        {
        }

        public ValidationErrorResult(string errorMessage, int statusCode) : base(new { Message = errorMessage })
        {
            StatusCode = statusCode;
        }

        public ValidationErrorResult(string errorMessage) : this(errorMessage, StatusCodes.Status400BadRequest)
        {
        }

        public override void OnFormatting(ActionContext context)
        {
            base.OnFormatting(context);
            context.HttpContext.Response.Headers["X-Status-Reason"] = "Validation failed";
        }
    }
}
