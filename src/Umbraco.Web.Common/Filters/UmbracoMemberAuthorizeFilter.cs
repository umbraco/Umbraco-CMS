using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Ensures authorization is successful for a front-end member.
/// </summary>
public class UmbracoMemberAuthorizeFilter : IAsyncAuthorizationFilter
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoMemberAuthorizeFilter" /> class
    ///     with empty allow lists.
    /// </summary>
    public UmbracoMemberAuthorizeFilter()
    {
        AllowType = string.Empty;
        AllowGroup = string.Empty;
        AllowMembers = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoMemberAuthorizeFilter" /> class
    ///     with specified allow lists.
    /// </summary>
    /// <param name="allowType">A comma delimited list of allowed member types.</param>
    /// <param name="allowGroup">A comma delimited list of allowed member groups.</param>
    /// <param name="allowMembers">A comma delimited list of allowed members.</param>
    public UmbracoMemberAuthorizeFilter(string allowType, string allowGroup, string allowMembers)
    {
        AllowType = allowType;
        AllowGroup = allowGroup;
        AllowMembers = allowMembers;
    }

    /// <summary>
    ///     Gets a comma delimited list of allowed member types.
    /// </summary>
    public string AllowType { get; private set; }

    /// <summary>
    ///     Gets a comma delimited list of allowed member groups.
    /// </summary>
    public string AllowGroup { get; private set; }

    /// <summary>
    ///     Gets a comma delimited list of allowed members.
    /// </summary>
    public string AllowMembers { get; private set; }

    /// <inheritdoc/>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Allow Anonymous skips all authorization
        if (HasAllowAnonymous(context))
        {
            return;
        }

        IMemberManager memberManager = context.HttpContext.RequestServices.GetRequiredService<IMemberManager>();

        if (memberManager.IsLoggedIn())
        {
            if (await IsAuthorizedAsync(memberManager) is false)
            {
                context.HttpContext.SetReasonPhrase(
                    "Resource restricted: the member is not of a permitted type or group.");

                if (IsApiController(context))
                {
                    // Return a raw 403 for API controllers so the cookie authentication handler's redirect
                    // behaviour is not triggered.
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
                else
                {
                    // For non-API controllers (e.g. SurfaceControllers), ForbidResult triggers the cookie authentication
                    // handler's OnRedirectToAccessDenied, which performs a redirect to the access denied page.
                    context.Result = new ForbidResult();
                }
            }
        }
        else
        {
            if (IsApiController(context))
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                // For non-API controllers (e.g. SurfaceControllers), ForbidResult triggers the cookie authentication
                // handler's OnRedirectToAccessDenied, which performs a redirect to the access denied page.
                // Using UnauthorizedResult here would bypass the authentication handler entirely, returning a raw 401
                // instead of the expected redirect.
                context.Result = new ForbidResult();
            }
        }
    }

    /// <summary>
    ///     Copied from https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Authorization/AuthorizeFilter.cs
    /// </summary>
    private bool HasAllowAnonymous(AuthorizationFilterContext context)
    {
        IList<IFilterMetadata> filters = context.Filters;
        for (var i = 0; i < filters.Count; i++)
        {
            if (filters[i] is IAllowAnonymousFilter)
            {
                return true;
            }
        }

        // When doing endpoint routing, MVC does not add AllowAnonymousFilters for AllowAnonymousAttributes that
        // were discovered on controllers and actions. To maintain compat with 2.x,
        // we'll check for the presence of IAllowAnonymous in endpoint metadata.
        Endpoint? endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return true;
        }

        return false;
    }

    private static bool IsApiController(AuthorizationFilterContext context)
        => context.ActionDescriptor.EndpointMetadata.OfType<ApiControllerAttribute>().Any()
#pragma warning disable CS0618 // Type or member is obsolete
           || (context.ActionDescriptor is ControllerActionDescriptor controllerDescriptor
               && controllerDescriptor.ControllerTypeInfo.IsSubclassOf(typeof(UmbracoApiController)));
#pragma warning restore CS0618 // Type or member is obsolete

    private async Task<bool> IsAuthorizedAsync(IMemberManager memberManager)
    {
        if (AllowMembers.IsNullOrWhiteSpace())
        {
            AllowMembers = string.Empty;
        }

        if (AllowGroup.IsNullOrWhiteSpace())
        {
            AllowGroup = string.Empty;
        }

        if (AllowType.IsNullOrWhiteSpace())
        {
            AllowType = string.Empty;
        }

        var members = new List<int>();
        foreach (var s in AllowMembers.Split(Core.Constants.CharArrays.Comma))
        {
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                members.Add(id);
            }
        }

        return await memberManager.IsMemberAuthorizedAsync(
            AllowType.Split(Core.Constants.CharArrays.Comma),
            AllowGroup.Split(Core.Constants.CharArrays.Comma),
            members);
    }
}
