using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.Security;
using UserExtensions = Umbraco.Core.Models.UserExtensions;

namespace Umbraco.Web.WebApi.Filters
{

    /// <summary>
    /// This filter will check if the current Principal/Identity assigned to the request has stale data in it compared
    /// to what is persisted for the current user and will update the current auth ticket with the correct data if required.
    /// </summary>
    public sealed class VerifyIfUserTicketDataIsStaleAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            await CheckStaleData(actionContext);
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await CheckStaleData(actionExecutedContext.ActionContext);
        }

        private async Task CheckStaleData(HttpActionContext actionContext)
        {
            var identity = actionContext.RequestContext.Principal.Identity as UmbracoBackOfficeIdentity;
            if (identity == null) return;

            var userId = identity.Id.TryConvertTo<int>();
            if (userId == false) return;

            var user = ApplicationContext.Current.Services.UserService.GetUserById(userId.Result);
            if (user == null) return;

            if (user.Username != identity.Username)
            {
                await ReSync(user, identity, actionContext);
                return;
            }

            var culture = UserExtensions.GetUserCulture(user, ApplicationContext.Current.Services.TextService).ToString();
            if (culture != identity.Culture)
            {
                //TODO: Might have to log out if this happens or somehow refresh the back office UI with a special header maybe?
                await ReSync(user, identity, actionContext);
                return;
            }

            if (user.AllowedSections.UnsortedSequenceEqual(identity.AllowedApplications) == false)
            {
                await ReSync(user, identity, actionContext);
                return;
            }

            if (user.Groups.Select(x => x.Alias).UnsortedSequenceEqual(identity.Roles) == false)
            {
                await ReSync(user, identity, actionContext);
                return;
            }

            //TODO: This will need to be changed when http://issues.umbraco.org/issue/U4-10173 is merged
            var startContentIds = user.AllStartContentIds;
            if (startContentIds.UnsortedSequenceEqual(identity.StartContentNodes) == false)
            {
                await ReSync(user, identity, actionContext);
                return;
            }

            //TODO: This will need to be changed when http://issues.umbraco.org/issue/U4-10173 is merged
            var startMediaIds = user.AllStartMediaIds;
            if (startMediaIds.UnsortedSequenceEqual(identity.StartMediaNodes) == false)
            {
                await ReSync(user, identity, actionContext);
                return;
            }
        }

        /// <summary>
        /// This will update the current request IPrincipal to be correct and re-create the auth ticket
        /// </summary>
        /// <param name="user"></param>
        /// <param name="identityUser"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private async Task ReSync(IUser user, UmbracoBackOfficeIdentity identityUser, HttpActionContext actionContext)
        {
            var owinCtx = actionContext.Request.TryGetOwinContext().Result;
            var signInManager = owinCtx.GetBackOfficeSignInManager();

            //ensure the remainder of the request has the correct principal set
            actionContext.Request.SetPrincipalForRequest(user);

            var backOfficeIdentityUser = Mapper.Map<BackOfficeIdentityUser>(user);
            await signInManager.SignInAsync(backOfficeIdentityUser, isPersistent: true, rememberBrowser: false);
        }
    }
}