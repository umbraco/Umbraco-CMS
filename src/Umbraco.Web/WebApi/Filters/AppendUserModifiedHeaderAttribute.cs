using System;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Appends a custom response header to notify the UI that the current user data has been modified
    /// </summary>
    public sealed class AppendUserModifiedHeaderAttribute : ActionFilterAttribute
    {
        private readonly string _userIdParameter;

        /// <summary>
        /// An empty constructor which will always set the header
        /// </summary>
        public AppendUserModifiedHeaderAttribute()
        {
        }

        /// <summary>
        /// A constructor specifying the action parameter name containing the user id to match against the current user and if they match the header will be appended
        /// </summary>
        /// <param name="userIdParameter"></param>
        public AppendUserModifiedHeaderAttribute(string userIdParameter)
        {
            if (userIdParameter == null) throw new ArgumentNullException("userIdParameter");
            _userIdParameter = userIdParameter;
        }

        public static void AppendHeader(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response.Headers.Contains("X-Umb-User-Modified") == false)
            {
                actionExecutedContext.Response.Headers.Add("X-Umb-User-Modified", "1");
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (_userIdParameter.IsNullOrWhiteSpace())
            {
                AppendHeader(actionExecutedContext);
            }
            else
            {
                var actionContext = actionExecutedContext.ActionContext;
                if (actionContext.ActionArguments[_userIdParameter] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _userIdParameter);
                }

                var user = UmbracoContext.Current.Security.CurrentUser;
                if (user == null) return;

                var userId = GetUserIdFromParameter(actionContext.ActionArguments[_userIdParameter]);

                if (userId == user.Id)
                    AppendHeader(actionExecutedContext);
            }

        }

        private int GetUserIdFromParameter(object parameterValue)
        {
            if (parameterValue is int)
            {
                return (int)parameterValue;
            }
            throw new InvalidOperationException("The id type: " + parameterValue.GetType() + " is not a supported id");
        }
    }
}
