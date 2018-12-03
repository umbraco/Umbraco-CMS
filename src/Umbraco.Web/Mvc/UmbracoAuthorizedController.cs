namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for authorized Umbraco controllers.
    /// </summary>
    /// <remarks>
    /// This controller essentially just uses a global UmbracoAuthorizeAttribute, inheritors that require more granular control over the
    /// authorization of each method can use this attribute instead of inheriting from this controller.
    /// </remarks>
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public abstract class UmbracoAuthorizedController : UmbracoController
    { }
}
