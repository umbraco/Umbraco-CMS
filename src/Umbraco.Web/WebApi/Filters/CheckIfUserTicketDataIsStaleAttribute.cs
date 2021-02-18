using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.Security;
using Umbraco.Core.Mapping;
using UserExtensions = Umbraco.Core.Models.UserExtensions;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This filter will check if the current Principal/Identity assigned to the request has stale data in it compared
    /// to what is persisted for the current user and will update the current auth ticket with the correct data if required and output
    /// a custom response header for the UI to be notified of it.
    /// </summary>
    /// <remarks>
    /// This could/should be created as a filter on the BackOfficeCookieAuthenticationProvider just like the SecurityStampValidator does
    /// </remarks>
    public sealed class CheckIfUserTicketDataIsStaleAttribute : ActionFilterAttribute
    {
        // this is an attribute - no choice
        private UmbracoMapper Mapper => Current.Mapper;

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            await CheckStaleData(actionContext);
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await CheckStaleData(actionExecutedContext.ActionContext);

            //we need new tokens and append the custom header if changes have been made
            if (actionExecutedContext.ActionContext.Request.Properties.ContainsKey(typeof(CheckIfUserTicketDataIsStaleAttribute).Name))
            {
                var tokenFilter = new SetAngularAntiForgeryTokensAttribute();
                tokenFilter.OnActionExecuted(actionExecutedContext);

                //add the header
                AppendUserModifiedHeaderAttribute.AppendHeader(actionExecutedContext);
            }
        }

        private async Task CheckStaleData(HttpActionContext actionContext)
        {
            if (actionContext == null
                || actionContext.Request == null
                || actionContext.RequestContext == null
                || actionContext.RequestContext.Principal == null
                || actionContext.RequestContext.Principal.Identity == null)
            {
                return;
            }

            //don't execute if it's already been done
            if (actionContext.Request.Properties.ContainsKey(typeof(CheckIfUserTicketDataIsStaleAttribute).Name))
                return;

            var identity = actionContext.RequestContext.Principal.Identity as UmbracoBackOfficeIdentity;
            if (identity == null) return;

            var userId = identity.Id.TryConvertTo<int>();
            if (userId == false) return;

            var user = Current.Services.UserService.GetUserById(userId.Result);
            if (user == null) return;

            //a list of checks to execute, if any of them pass then we resync
            var checks = new Func<bool>[]
            {
                () => user.Username != identity.Username,
                () =>
                {
                    var culture = UserExtensions.GetUserCulture(user, Current.Services.TextService, Current.Configs.Global());
                    return culture != null && culture.ToString() != identity.Culture;
                },
                () => user.AllowedSections.UnsortedSequenceEqual(identity.AllowedApplications) == false,
                () => user.Groups.Select(x => x.Alias).UnsortedSequenceEqual(identity.Roles) == false,
                () =>
                {
                    var startContentIds = UserExtensions.CalculateContentStartNodeIds(user, Current.Services.EntityService, Current.AppCaches);
                    return startContentIds.UnsortedSequenceEqual(identity.StartContentNodes) == false;
                },
                () =>
                {
                    var startMediaIds = UserExtensions.CalculateMediaStartNodeIds(user, Current.Services.EntityService, Current.AppCaches);
                    return startMediaIds.UnsortedSequenceEqual(identity.StartMediaNodes) == false;
                }
            };

            if (checks.Any(check => check()))
            {
                await ReSync(user, actionContext);
            }
        }

        /// <summary>
        /// This will update the current request IPrincipal to be correct and re-create the auth ticket
        /// </summary>
        /// <param name="user"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private async Task ReSync(IUser user, HttpActionContext actionContext)
        {
            var owinCtx = actionContext.Request.TryGetOwinContext();
            if (owinCtx)
            {
                var signInManager = owinCtx.Result.GetBackOfficeSignInManager();

                var backOfficeIdentityUser = Mapper.Map<BackOfficeIdentityUser>(user);
                await signInManager.SignInAsync(backOfficeIdentityUser, isPersistent: true, rememberBrowser: false);

                //ensure the remainder of the request has the correct principal set
                actionContext.Request.SetPrincipalForRequest(owinCtx.Result.Request.User);

                //flag that we've made changes
                actionContext.Request.Properties[typeof(CheckIfUserTicketDataIsStaleAttribute).Name] = true;
            }
        }
    }
}
