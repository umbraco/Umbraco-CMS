using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CheckIfUserTicketDataIsStaleAttribute : TypeFilterAttribute
    {
        public CheckIfUserTicketDataIsStaleAttribute() : base(typeof(CheckIfUserTicketDataIsStaleFilter))
        {
        }

        private class CheckIfUserTicketDataIsStaleFilter : IAsyncActionFilter
        {
            private readonly IRequestCache _requestCache;
            private readonly UmbracoMapper _umbracoMapper;
            private readonly IUserService _userService;
            private readonly IEntityService _entityService;
            private readonly ILocalizedTextService _localizedTextService;
            private readonly IOptions<GlobalSettings> _globalSettings;
            private readonly IBackOfficeSignInManager _backOfficeSignInManager;
            private readonly IBackOfficeAntiforgery _backOfficeAntiforgery;

            public CheckIfUserTicketDataIsStaleFilter(
                IRequestCache requestCache,
                UmbracoMapper umbracoMapper,
                IUserService userService,
                IEntityService entityService,
                ILocalizedTextService localizedTextService,
                IOptions<GlobalSettings> globalSettings,
                IBackOfficeSignInManager backOfficeSignInManager,
                IBackOfficeAntiforgery backOfficeAntiforgery)
            {
                _requestCache = requestCache;
                _umbracoMapper = umbracoMapper;
                _userService = userService;
                _entityService = entityService;
                _localizedTextService = localizedTextService;
                _globalSettings = globalSettings;
                _backOfficeSignInManager = backOfficeSignInManager;
                _backOfficeAntiforgery = backOfficeAntiforgery;
            }


            public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
            {
                await CheckStaleData(actionContext);

                await next();

                await CheckStaleData(actionContext);

                //return if nothing is updated
                if (_requestCache.Get(nameof(CheckIfUserTicketDataIsStaleFilter)) is null)
                    return;

                await UpdateTokensAndAppendCustomHeaders(actionContext);
            }

            private async Task UpdateTokensAndAppendCustomHeaders(ActionExecutingContext actionContext)
            {
                var tokenFilter =
                    new SetAngularAntiForgeryTokensAttribute.SetAngularAntiForgeryTokensFilter(_backOfficeAntiforgery,
                        _globalSettings);
                await tokenFilter.OnActionExecutionAsync(actionContext,
                    () => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null)));

                //add the header
                AppendUserModifiedHeaderAttribute.AppendHeader(actionContext);
            }


            private async Task CheckStaleData(ActionExecutingContext actionContext)
            {
                if (actionContext?.HttpContext.Request == null || actionContext.HttpContext.User?.Identity == null)
                {
                    return;
                }

                //don't execute if it's already been done
                if (!(_requestCache.Get(nameof(CheckIfUserTicketDataIsStaleFilter)) is null))
                    return;

                var identity = actionContext.HttpContext.User.Identity as UmbracoBackOfficeIdentity;
                if (identity == null) return;

                var userId = identity.Id.TryConvertTo<int>();
                if (userId == false) return;

                var user = _userService.GetUserById(userId.Result);
                if (user == null) return;

                //a list of checks to execute, if any of them pass then we resync
                var checks = new Func<bool>[]
                {
                    () => user.Username != identity.Username,
                    () =>
                    {
                        var culture = user.GetUserCulture(_localizedTextService, _globalSettings.Value);
                        return culture != null && culture.ToString() != identity.Culture;
                    },
                    () => user.AllowedSections.UnsortedSequenceEqual(identity.AllowedApplications) == false,
                    () => user.Groups.Select(x => x.Alias).UnsortedSequenceEqual(identity.Roles) == false,
                    () =>
                    {
                        var startContentIds = user.CalculateContentStartNodeIds(_entityService);
                        return startContentIds.UnsortedSequenceEqual(identity.StartContentNodes) == false;
                    },
                    () =>
                    {
                        var startMediaIds = user.CalculateMediaStartNodeIds(_entityService);
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
            private async Task ReSync(IUser user, ActionExecutingContext actionContext)
            {
                var backOfficeIdentityUser = _umbracoMapper.Map<BackOfficeIdentityUser>(user);
                await _backOfficeSignInManager.SignInAsync(backOfficeIdentityUser, isPersistent: true);

                //ensure the remainder of the request has the correct principal set
                actionContext.HttpContext.SetPrincipalForRequest(ClaimsPrincipal.Current);

                //flag that we've made changes
                _requestCache.Set(nameof(CheckIfUserTicketDataIsStaleFilter), true);
            }
        }
    }
}
