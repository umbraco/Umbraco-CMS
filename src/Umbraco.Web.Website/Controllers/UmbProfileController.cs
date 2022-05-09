using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers;

[UmbracoMemberAuthorize]
public class UmbProfileController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ICoreScopeProvider _scopeProvider;

    public UmbProfileController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        ICoreScopeProvider scopeProvider)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _scopeProvider = scopeProvider;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        MemberIdentityUser? currentMember = await _memberManager.GetUserAsync(HttpContext.User);
        if (currentMember == null!)
        {
            // this shouldn't happen, we also don't want to return an error so just redirect to where we came from
            return RedirectToCurrentUmbracoPage();
        }

        IdentityResult result = await UpdateMemberAsync(model, currentMember);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return CurrentUmbracoPage();
        }

        TempData["FormSuccess"] = true;

        // If there is a specified path to redirect to then use it.
        if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
        {
            return Redirect(model.RedirectUrl!);
        }

        // Redirect to current page by default.
        return RedirectToCurrentUmbracoPage();
    }

    /// <summary>
    ///     We pass in values via encrypted route values so they cannot be tampered with and merge them into the model for use
    /// </summary>
    /// <param name="model"></param>
    private void MergeRouteValuesToModel(ProfileModel model)
    {
        if (RouteData.Values.TryGetValue(nameof(ProfileModel.RedirectUrl), out var redirectUrl) && redirectUrl != null)
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (IdentityError? error in result.Errors)
        {
            ModelState.AddModelError("profileModel", error.Description);
        }
    }

    private async Task<IdentityResult> UpdateMemberAsync(ProfileModel model, MemberIdentityUser currentMember)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        currentMember.Email = model.Email;
        currentMember.Name = model.Name;
        currentMember.UserName = model.UserName;
        currentMember.Comments = model.Comments;

        IdentityResult saveResult = await _memberManager.UpdateAsync(currentMember);
        if (!saveResult.Succeeded)
        {
            return saveResult;
        }

        // now we can update the custom properties
        // TODO: Ideally we could do this all through our MemberIdentityUser
        IMember? member = _memberService.GetByKey(currentMember.Key);
        if (member == null)
        {
            // should never happen
            throw new InvalidOperationException($"Could not find a member with key: {member?.Key}.");
        }

        IMemberType? memberType = _memberTypeService.Get(member.ContentTypeId);

        foreach (MemberPropertyModel property in model.MemberProperties

                     // ensure the property they are posting exists
                     .Where(p => memberType?.PropertyTypeExists(p.Alias) ?? false)
                     .Where(property => member.Properties.Contains(property.Alias))

                     // needs to be editable
                     .Where(p => memberType?.MemberCanEditProperty(p.Alias) ?? false))
        {
            member.Properties[property.Alias]?.SetValue(property.Value);
        }

        _memberService.Save(member);

        return saveResult;
    }
}
