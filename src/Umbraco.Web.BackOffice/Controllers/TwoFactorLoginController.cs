using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

public class TwoFactorLoginController : UmbracoAuthorizedJsonController
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly ILogger<TwoFactorLoginController> _logger;
    private readonly ITwoFactorLoginService2 _twoFactorLoginService;
    private readonly IOptionsSnapshot<TwoFactorLoginViewOptions> _twoFactorLoginViewOptions;

    public TwoFactorLoginController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILogger<TwoFactorLoginController> logger,
        ITwoFactorLoginService twoFactorLoginService,
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager,
        IOptionsSnapshot<TwoFactorLoginViewOptions> twoFactorLoginViewOptions)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _logger = logger;

        if (twoFactorLoginService is not ITwoFactorLoginService2 twoFactorLoginService2)
        {
            throw new ArgumentException(
                "twoFactorLoginService needs to implement ITwoFactorLoginService2 until the interfaces are merged",
                nameof(twoFactorLoginService));
        }

        _twoFactorLoginService = twoFactorLoginService2;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
        _twoFactorLoginViewOptions = twoFactorLoginViewOptions;
    }

    /// <summary>
    ///     Used to retrieve the 2FA providers for code submission
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetEnabled2FAProvidersForCurrentUser()
    {
        BackOfficeIdentityUser? user = await _backOfficeSignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            _logger.LogWarning("No verified user found, returning 404");
            return NotFound();
        }

        IList<string> userFactors = await _backOfficeUserManager.GetValidTwoFactorProvidersAsync(user);
        return new ObjectResult(userFactors);
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserTwoFactorProviderModel>>> Get2FAProvidersForUser(int userId)
    {
        BackOfficeIdentityUser user = await _backOfficeUserManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));

        var enabledProviderNameHashSet =
            new HashSet<string>(await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(user.Key));

        IEnumerable<string> providerNames = await _backOfficeUserManager.GetValidTwoFactorProvidersAsync(user);

        // Filter out any providers that does not have a view attached to it, since it's unusable then.
        providerNames = providerNames.Where(providerName =>
        {
            TwoFactorLoginViewOptions options = _twoFactorLoginViewOptions.Get(providerName);
            return options is not null && !string.IsNullOrWhiteSpace(options.SetupViewPath);
        });

        return providerNames.Select(providerName =>
            new UserTwoFactorProviderModel(providerName, enabledProviderNameHashSet.Contains(providerName))).ToArray();
    }

    [HttpGet]
    public async Task<ActionResult<object?>> SetupInfo(string providerName)
    {
        IUser? user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

        var setupInfo = await _twoFactorLoginService.GetSetupInfoAsync(user!.Key, providerName);

        return setupInfo;
    }


    [HttpPost]
    public async Task<ActionResult<bool>> ValidateAndSave(string providerName, string secret, string code)
    {
        IUser? user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

        return await _twoFactorLoginService.ValidateAndSaveAsync(providerName, user!.Key, secret, code);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
    public async Task<ActionResult<bool>> Disable(string providerName, Guid userKey) =>
        await _twoFactorLoginService.DisableAsync(userKey, providerName);

    [HttpPost]
    public async Task<ActionResult<bool>> DisableWithCode(string providerName, string code)
    {
        Guid? key = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key;

        return await _twoFactorLoginService.DisableWithCodeAsync(providerName, key!.Value, code);
    }

    [HttpGet]
    public ActionResult<string?> ViewPathForProviderName(string providerName)
    {
        TwoFactorLoginViewOptions? options = _twoFactorLoginViewOptions.Get(providerName);
        return options.SetupViewPath;
    }
}
