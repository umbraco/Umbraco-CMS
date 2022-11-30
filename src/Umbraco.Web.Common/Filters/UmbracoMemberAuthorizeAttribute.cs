using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Ensures authorization is successful for a website user (member).
/// </summary>
public class UmbracoMemberAuthorizeAttribute : TypeFilterAttribute
{
    public UmbracoMemberAuthorizeAttribute()
        : this(string.Empty, string.Empty, string.Empty)
    {
    }

    public UmbracoMemberAuthorizeAttribute(string allowType, string allowGroup, string allowMembers)
        : base(typeof(UmbracoMemberAuthorizeFilter)) => Arguments = new object[] { allowType, allowGroup, allowMembers };
}
