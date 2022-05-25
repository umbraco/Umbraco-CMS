using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     This inspects the result of the action that returns a collection of content and removes
///     any item that the current user doesn't have access to
/// </summary>
internal class FilterAllowedOutgoingMediaAttribute : TypeFilterAttribute
{
    public FilterAllowedOutgoingMediaAttribute(Type outgoingType, string? propertyName = null)
        : base(typeof(FilterAllowedOutgoingMediaFilter)) =>
        Arguments = new object[] { outgoingType, propertyName ?? string.Empty };
}

internal class FilterAllowedOutgoingMediaFilter : IActionFilter
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly Type _outgoingType;
    private readonly string _propertyName;

    public FilterAllowedOutgoingMediaFilter(
        IEntityService entityService,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        AppCaches appCaches,
        Type outgoingType,
        string propertyName)
    {
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _appCaches = appCaches;

        _propertyName = propertyName;
        _outgoingType = outgoingType;
    }

    protected virtual int RecycleBinId => Constants.System.RecycleBinMedia;

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result == null)
        {
            return;
        }

        IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (user == null)
        {
            return;
        }

        if (context.Result is ObjectResult objectContent)
        {
            dynamic? collection = GetValueFromResponse(objectContent);

            if (collection != null)
            {
                var items = Enumerable.ToList(collection);

                FilterItems(user, items);

                //set the return value
                SetValueForResponse(objectContent, items);
            }
        }
    }


    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    protected virtual int[]? GetUserStartNodes(IUser user) =>
        user.CalculateMediaStartNodeIds(_entityService, _appCaches);

    protected virtual void FilterItems(IUser user, IList items) => FilterBasedOnStartNode(items, user);

    internal void FilterBasedOnStartNode(IList items, IUser user)
    {
        var toRemove = new List<dynamic>();
        foreach (dynamic item in items)
        {
            dynamic hasPathAccess = item != null &&
                                ContentPermissions.HasPathAccess(item?.Path, GetUserStartNodes(user), RecycleBinId);
            if (hasPathAccess == false)
            {
                toRemove.Add(item);
            }
        }

        foreach (dynamic item in toRemove)
        {
            items.Remove(item);
        }
    }

    private void SetValueForResponse(ObjectResult objectContent, dynamic newVal)
    {
        if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value?.GetType()))
        {
            objectContent.Value = newVal;
        }
        else if (_propertyName.IsNullOrWhiteSpace() == false)
        {
            //try to get the enumerable collection from a property on the result object using reflection
            PropertyInfo? property = objectContent.Value?.GetType().GetProperty(_propertyName);
            if (property != null)
            {
                property.SetValue(objectContent.Value, newVal);
            }
        }
    }

    internal dynamic? GetValueFromResponse(ObjectResult objectContent)
    {
        if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value?.GetType()))
        {
            return objectContent.Value;
        }

        if (_propertyName.IsNullOrWhiteSpace() == false)
        {
            //try to get the enumerable collection from a property on the result object using reflection
            PropertyInfo? property = objectContent.Value?.GetType().GetProperty(_propertyName);
            if (property != null)
            {
                var result = property.GetValue(objectContent.Value);
                if (result != null && TypeHelper.IsTypeAssignableFrom(_outgoingType, result.GetType()))
                {
                    return result;
                }
            }
        }

        return null;
    }
}
