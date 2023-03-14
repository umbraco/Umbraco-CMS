using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
/// </summary>
internal sealed class CheckIfUserTicketDataIsStaleAttribute : TypeFilterAttribute
{
    public CheckIfUserTicketDataIsStaleAttribute()
        : base(typeof(CheckIfUserTicketDataIsStaleFilter))
    {
    }

    private class CheckIfUserTicketDataIsStaleFilter : IAsyncActionFilter
    {
        private readonly AppCaches _appCaches;
        private readonly IBackOfficeAntiforgery _backOfficeAntiforgery;
        private readonly IBackOfficeSignInManager _backOfficeSignInManager;
        private readonly IEntityService _entityService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IRequestCache _requestCache;
        private readonly ICoreScopeProvider _scopeProvider;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IUserService _userService;
        private readonly GlobalSettings _globalSettings;

        public CheckIfUserTicketDataIsStaleFilter(
            IRequestCache requestCache,
            IUmbracoMapper umbracoMapper,
            IUserService userService,
            IEntityService entityService,
            ILocalizedTextService localizedTextService,
            IOptionsSnapshot<GlobalSettings> globalSettings,
            IBackOfficeSignInManager backOfficeSignInManager,
            IBackOfficeAntiforgery backOfficeAntiforgery,
            ICoreScopeProvider scopeProvider,
            AppCaches appCaches)
        {
            _requestCache = requestCache;
            _umbracoMapper = umbracoMapper;
            _userService = userService;
            _entityService = entityService;
            _localizedTextService = localizedTextService;
            _globalSettings = globalSettings.Value;
            _backOfficeSignInManager = backOfficeSignInManager;
            _backOfficeAntiforgery = backOfficeAntiforgery;
            _scopeProvider = scopeProvider;
            _appCaches = appCaches;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            await CheckStaleData(actionContext);

            await next();

            await CheckStaleData(actionContext);

            // return if nothing is updated
            if (_requestCache.Get(nameof(CheckIfUserTicketDataIsStaleFilter)) is null)
            {
                return;
            }

            await UpdateTokensAndAppendCustomHeaders(actionContext);
        }

        private async Task UpdateTokensAndAppendCustomHeaders(ActionExecutingContext actionContext)
        {
            var tokenFilter =
                new SetAngularAntiForgeryTokensAttribute.SetAngularAntiForgeryTokensFilter(_backOfficeAntiforgery);
            await tokenFilter.OnActionExecutionAsync(
                actionContext,
                () => Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new { })));

            // add the header
            AppendUserModifiedHeaderAttribute.AppendHeader(actionContext);
        }


        private async Task CheckStaleData(ActionExecutingContext actionContext)
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
            {
                if (actionContext?.HttpContext.Request == null || actionContext.HttpContext.User?.Identity == null)
                {
                    return;
                }

                // don't execute if it's already been done
                if (!(_requestCache.Get(nameof(CheckIfUserTicketDataIsStaleFilter)) is null))
                {
                    return;
                }

                if (actionContext.HttpContext.User.Identity is not ClaimsIdentity identity)
                {
                    return;
                }

                var id = identity.GetId();
                if (id is null)
                {
                    return;
                }

                IUser? user = _userService.GetUserById(id.Value);
                if (user == null)
                {
                    return;
                }

                // a list of checks to execute, if any of them pass then we resync
                var checks = new Func<bool>[]
                {
                    () => user.Username != identity.GetUsername(), () =>
                    {
                        CultureInfo culture = user.GetUserCulture(_localizedTextService, _globalSettings);
                        return culture != null && culture.ToString() != identity.GetCultureString();
                    },
                    () => user.AllowedSections.UnsortedSequenceEqual(identity.GetAllowedApplications()) == false,
                    () => user.Groups.Select(x => x.Alias).UnsortedSequenceEqual(identity.GetRoles()) == false, () =>
                    {
                        var startContentIds = user.CalculateContentStartNodeIds(_entityService, _appCaches);
                        return startContentIds.UnsortedSequenceEqual(identity.GetStartContentNodes()) == false;
                    },
                    () =>
                    {
                        var startMediaIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
                        return startMediaIds.UnsortedSequenceEqual(identity.GetStartMediaNodes()) == false;
                    }
                };

                if (checks.Any(check => check()))
                {
                    await ReSync(user, actionContext);
                }
            }
        }

        /// <summary>
        ///     This will update the current request IPrincipal to be correct and re-create the auth ticket
        /// </summary>
        private async Task ReSync(IUser user, ActionExecutingContext actionContext)
        {
            BackOfficeIdentityUser? backOfficeIdentityUser = _umbracoMapper.Map<BackOfficeIdentityUser>(user);
            if (backOfficeIdentityUser is not null)
            {
                await _backOfficeSignInManager.SignInAsync(backOfficeIdentityUser, true);
            }

            // flag that we've made changes
            _requestCache.Set(nameof(CheckIfUserTicketDataIsStaleFilter), true);
        }
    }
}
