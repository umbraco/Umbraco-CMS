using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Ensures authorization is successful for a front-end member
/// </summary>
public class UmbracoMemberAuthorizeFilter : IAsyncAuthorizationFilter
{
    public UmbracoMemberAuthorizeFilter()
    {
        AllowType = string.Empty;
        AllowGroup = string.Empty;
        AllowMembers = string.Empty;
    }

    public UmbracoMemberAuthorizeFilter(string allowType, string allowGroup, string allowMembers)
    {
        AllowType = allowType;
        AllowGroup = allowGroup;
        AllowMembers = allowMembers;
    }

    /// <summary>
    ///     Comma delimited list of allowed member types
    /// </summary>
    public string AllowType { get; private set; }

    /// <summary>
    ///     Comma delimited list of allowed member groups
    /// </summary>
    public string AllowGroup { get; private set; }

    /// <summary>
    ///     Comma delimited list of allowed members
    /// </summary>
    public string AllowMembers { get; private set; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Allow Anonymous skips all authorization
        if (HasAllowAnonymous(context))
        {
            return;
        }

        IMemberManager memberManager = context.HttpContext.RequestServices.GetRequiredService<IMemberManager>();

        if (!await IsAuthorizedAsync(memberManager))
        {
            context.HttpContext.SetReasonPhrase(
                "Resource restricted: either member is not logged on or is not of a permitted type or group.");
            context.Result = new ForbidResult();
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
