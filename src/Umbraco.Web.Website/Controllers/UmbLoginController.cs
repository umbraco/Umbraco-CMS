using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Web.Website.Controllers;

public class UmbLoginController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberSignInManager _signInManager;
    private readonly ITwoFactorLoginService _twoFactorLoginService;
    private readonly IEmailSender _emailSender;
    private readonly GlobalSettings _globalSettings;

    [ActivatorUtilitiesConstructor]
    public UmbLoginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager,
        IMemberManager memberManager,
        ITwoFactorLoginService twoFactorLoginService,
        IEmailSender emailSender,
        IOptions<GlobalSettings> globalSettings)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _signInManager = signInManager;
        _memberManager = memberManager;
        _twoFactorLoginService = twoFactorLoginService;
        _emailSender = emailSender;
        _globalSettings = globalSettings.Value;
    }

    [Obsolete("Use ctor with all params")]
    public UmbLoginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberSignInManager signInManager)
        : this(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider,
            signInManager,
            StaticServiceProvider.Instance.GetRequiredService<IMemberManager>(),
            StaticServiceProvider.Instance.GetRequiredService<ITwoFactorLoginService>(),
            StaticServiceProvider.Instance.GetRequiredService<IEmailSender>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>())
    {
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleForgottenPassword([Bind(Prefix = "forgottenPasswordModel")] ForgottenPasswordModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        if (!_emailSender.CanSendRequiredEmail())
        {
            return CurrentUmbracoPage();
        }

        MemberIdentityUser? memberIdentity = await _memberManager.FindByNameAsync(model.Username);
        if (memberIdentity == null!)
        {
            return new ValidationErrorResult(
                $"No local member found for username {model.Username}");
        }

        string token = await _memberManager.GeneratePasswordResetTokenAsync(memberIdentity);

        var inviteToken = $"{memberIdentity.Id}{WebUtility.UrlEncode("|")}{token.ToUrlBase64()}";

        var senderEmail = _globalSettings.Smtp?.From;

        var passwordLink = $"<a href=\"{model.RedirectUrl}?invite={inviteToken}\">Reset password</a>";

        var subject = "Reset Password";
        var body = $"<h3>Reset password</h3>"
            + $"<p>{passwordLink}</p>";

        var mailMessage = new EmailMessage(senderEmail, memberIdentity.Email, subject, body, true);

        await _emailSender.SendAsync(mailMessage, Core.Constants.Web.EmailTypes.PasswordReset);

        TempData["ForgottenPasswordSuccess"] = true;

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleResetPassword([Bind(Prefix = "resetPasswordModel")] ResetPasswordModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        string? token = model.Token.FromUrlBase64();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(model.UserId))
        {
            return CurrentUmbracoPage();
        }

        MemberIdentityUser? memberIdentity = await _memberManager.FindByIdAsync(model.UserId);
        if (memberIdentity == null!)
        {
            return new ValidationErrorResult(
                $"No local member found for user id {model.UserId}");
        }

        IdentityResult result = await _memberManager.ChangePasswordWithResetAsync(memberIdentity.Id, token, model.Password);

        if (result.Succeeded)
        {
            TempData["ResetPasswordSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // Validate the redirect URL.
                // If it's not a local URL we'll redirect to the root of the current site.
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage!.AncestorOrSelf(1)!.Url(PublishedUrlProvider));
            }

            // Redirect to current URL by default.
            // This is different from the current 'page' because when using Public Access the current page
            // will be the login page, but the URL will be on the requested page so that's where we need
            // to redirect too.
            return RedirectToCurrentUmbracoUrl();
        }

        return CurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleLogin([Bind(Prefix = "loginModel")] LoginModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        // Sign the user in with username/password, this also gives a chance for developers to
        // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
        SignInResult result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, true);

        if (result.Succeeded)
        {
            TempData["LoginSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // Validate the redirect URL.
                // If it's not a local URL we'll redirect to the root of the current site.
                return Redirect(Url.IsLocalUrl(model.RedirectUrl)
                    ? model.RedirectUrl
                    : CurrentPage!.AncestorOrSelf(1)!.Url(PublishedUrlProvider));
            }

            // Redirect to current URL by default.
            // This is different from the current 'page' because when using Public Access the current page
            // will be the login page, but the URL will be on the requested page so that's where we need
            // to redirect too.
            return RedirectToCurrentUmbracoUrl();
        }

        if (result.RequiresTwoFactor)
        {
            MemberIdentityUser? attemptedUser = await _memberManager.FindByNameAsync(model.Username);
            if (attemptedUser == null!)
            {
                return new ValidationErrorResult(
                    $"No local member found for username {model.Username}");
            }

            IEnumerable<string> providerNames =
                await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(attemptedUser.Key);
            ViewData.SetTwoFactorProviderNames(providerNames);
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError("loginModel", "Member is locked out");
        }
        else if (result.IsNotAllowed)
        {
            ModelState.AddModelError("loginModel", "Member is not allowed");
        }
        else
        {
            ModelState.AddModelError("loginModel", "Invalid username or password");
        }

        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     We pass in values via encrypted route values so they cannot be tampered with and merge them into the model for use.
    /// </summary>
    /// <param name="model"></param>
    private void MergeRouteValuesToModel(PostRedirectModel model)
    {
        if (RouteData.Values.TryGetValue(nameof(PostRedirectModel.RedirectUrl), out var redirectUrl) && redirectUrl != null)
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }
}
