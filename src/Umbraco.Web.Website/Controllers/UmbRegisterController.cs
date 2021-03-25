using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Security;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Controllers
{
    public class UmbRegisterController : SurfaceController
    {
        private readonly MemberManager _memberManager;
        private readonly IMemberService _memberService;

        public UmbRegisterController(
            MemberManager memberManager,
            IMemberService memberService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            // U4-10762 Server error with "Register Member" snippet (Cannot save member with empty name)
            // If name field is empty, add the email address instead.
            if (string.IsNullOrEmpty(model.Name) && string.IsNullOrEmpty(model.Email) == false)
            {
                model.Name = model.Email;
            }

            IdentityResult result = await RegisterMemberAsync(model, model.LoginOnSuccess);
            if (result.Succeeded)
            {
                TempData["FormSuccess"] = true;

                // If there is a specified path to redirect to then use it.
                if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
                {
                    return Redirect(model.RedirectUrl);
                }

                // Redirect to current page by default.
                return RedirectToCurrentUmbracoPage();
            }
            else
            {
                AddModelErrors(result, "registerModel");
                return CurrentUmbracoPage();
            }
        }

        private void AddModelErrors(IdentityResult result, string prefix = "")
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(prefix, error.Description);
            }
        }

        /// <summary>
        /// Registers a new member.
        /// </summary>
        /// <param name="model">Register member model.</param>
        /// <param name="logMemberIn">Flag for whether to log the member in upon successful registration.</param>
        /// <returns>Result of registration operation.</returns>
        private async Task<IdentityResult> RegisterMemberAsync(RegisterModel model, bool logMemberIn = true)
        {
            model.Username = (model.UsernameIsEmail || model.Username == null) ? model.Email : model.Username;

            var identityUser = MembersIdentityUser.CreateNew(model.Username, model.Email, model.MemberTypeAlias, model.Name);
            IdentityResult identityResult = await _memberManager.CreateAsync(
                identityUser,
                model.Password);

            if (identityResult.Succeeded)
            {
                // Update the custom properties
                // TODO: See TODO in MembersIdentityUser, Should we support custom member properties for persistence/retrieval?
                IMember member = _memberService.GetByUsername(identityUser.UserName);
                if (model.MemberProperties != null)
                {
                    foreach (UmbracoProperty property in model.MemberProperties.Where(p => p.Value != null)
                        .Where(property => member.Properties.Contains(property.Alias)))
                    {
                        member.Properties[property.Alias].SetValue(property.Value);
                    }
                }
                _memberService.Save(member);

                if (logMemberIn)
                {
                    // TODO: Log them in
                    throw new NotImplementedException("Implement MemberSignInManager");
                }
            }

            return identityResult;

        }
    }
}
