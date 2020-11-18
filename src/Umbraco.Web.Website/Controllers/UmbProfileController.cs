using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Security;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Website.Controllers
{
    // TOOO: reinstate [MemberAuthorize]
    public class UmbProfileController : SurfaceController
    {
        private readonly IUmbracoWebsiteSecurity _websiteSecurity;

        public UmbProfileController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider, IUmbracoWebsiteSecurity websiteSecurity)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _websiteSecurity = websiteSecurity;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            _websiteSecurity.UpdateMemberProfile(model, out var status, out var errorMessage);
            switch(status)
            {
                case UpdateMemberProfileStatus.Success:
                    break;
                case UpdateMemberProfileStatus.Error:
                    // Don't add a field level error, just model level.
                    ModelState.AddModelError("profileModel", errorMessage);
                    return CurrentUmbracoPage();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TempData["ProfileUpdateSuccess"] = true;

            // If there is a specified path to redirect to then use it.
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                return Redirect(model.RedirectUrl);
            }

            // Redirect to current page by default.
            return RedirectToCurrentUmbracoPage();
        }
    }
}
