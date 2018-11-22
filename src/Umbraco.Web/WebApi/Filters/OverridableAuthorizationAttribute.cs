using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Abstract auth filter class that can be used to enable overriding class auth filters at the action level
    /// </summary>
    /// <remarks>
    /// To enable a class auth filter to be overridden by an action auth filter the EnableOverrideAuthorizationAttribute can be applied 
    /// to the class.
    /// </remarks>
    public abstract class OverridableAuthorizationAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// If the controller has an EnabledOverrideAuthorizationAttribute attribute specified and the action has any AuthorizeAttribute
        /// specified then use the action's auth attribute instead of this one
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <exception cref="T:System.ArgumentNullException">The context parameter is null.</exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null) throw new ArgumentNullException("actionContext");

            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>();

            //if 'this' authorize attribute exists in the current collection then continue as per normal... this is because 'this' attribute instance
            // is obviously assigned at an Action level and therefore it's already executing

            if (actionAttributes.Any(x => Equals(x, this)))
            {
                base.OnAuthorization(actionContext);
                return;
            }

            //if the controller is allowing overridable authorization at the action level and there are action level authorization attributes
            // then exit and let the action level auth attribute(s) execute.

            if (actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<EnableOverrideAuthorizationAttribute>().Any()
                && actionAttributes.Any())
            {
                return;
            }

            base.OnAuthorization(actionContext);
        }
    }
}