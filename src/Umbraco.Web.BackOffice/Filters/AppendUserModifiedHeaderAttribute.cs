using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Appends a custom response header to notify the UI that the current user data has been modified
/// </summary>
public sealed class AppendUserModifiedHeaderAttribute : ActionFilterAttribute
{
    private readonly string? _userIdParameter;

    /// <summary>
    ///     An empty constructor which will always set the header.
    /// </summary>
    public AppendUserModifiedHeaderAttribute()
    {
    }

    /// <summary>
    ///     A constructor specifying the action parameter name containing the user id to match against the
    ///     current user and if they match the header will be appended.
    /// </summary>
    /// <param name="userIdParameter"></param>
    public AppendUserModifiedHeaderAttribute(string userIdParameter) => _userIdParameter =
        userIdParameter ?? throw new ArgumentNullException(nameof(userIdParameter));

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (_userIdParameter.IsNullOrWhiteSpace())
        {
            AppendHeader(context);
        }
        else
        {
            if (!context.ActionArguments.ContainsKey(_userIdParameter!))
            {
                throw new InvalidOperationException(
                    $"No argument found for the current action with the name: {_userIdParameter}");
            }

            IBackOfficeSecurityAccessor? backofficeSecurityAccessor =
                context.HttpContext.RequestServices.GetService<IBackOfficeSecurityAccessor>();
            IUser? user = backofficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;
            if (user == null)
            {
                return;
            }

            var userId = GetUserIdFromParameter(context.ActionArguments[_userIdParameter!]);
            if (userId == user.Id)
            {
                AppendHeader(context);
            }
        }
    }

    public static void AppendHeader(ActionExecutingContext context)
    {
        const string HeaderName = "X-Umb-User-Modified";
        if (context.HttpContext.Response.Headers.ContainsKey(HeaderName) == false)
        {
            context.HttpContext.Response.Headers.Add(HeaderName, "1");
        }
    }

    private int GetUserIdFromParameter(object? parameterValue)
    {
        if (parameterValue is int)
        {
            return (int)parameterValue;
        }

        throw new InvalidOperationException($"The id type: {parameterValue?.GetType()} is not a supported id.");
    }
}
