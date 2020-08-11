using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Web.Common.ActionsResults
{
    /// <summary>
    /// Custom result to return a validation error message with a 400 http response and required headers
    /// </summary>
    public class ValidationErrorResult : ObjectResult
    {
        public ValidationErrorResult(string errorMessage) : base(new { Message = errorMessage })
        {
            StatusCode = (int)HttpStatusCode.BadRequest;           
        }

        public override void OnFormatting(ActionContext context)
        {
            base.OnFormatting(context);
            context.HttpContext.Response.Headers["X-Status-Reason"] = "Validation failed";
        }
    }
}
