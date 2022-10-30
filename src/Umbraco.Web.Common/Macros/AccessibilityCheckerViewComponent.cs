using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Preview;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Macros
{
    public class AccessibilityCheckerViewComponent : ViewComponent
    {
        private readonly ILocalizationService _localizationService;
        public AccessibilityCheckerViewComponent(ILocalizationService localizationService)
        {
            _localizationService = localizationService;    
        }
        [HttpGet]
        public IViewComponentResult Invoke()
        {
            // can not inject umbraco context
            var isPreview = Request.HasPreviewCookie()
                && !Request.IsBackOfficeRequest();

            if (!isPreview)
            {
                return Content(string.Empty);
            }
            // Accessiblity checker does not work in canvas designer mode, this is due to the fact that the canvas designer is a page in a page.
            if (TempData.ContainsKey("CanvasDesigner"))
            {
                TempData.Remove("CanvasDesigner");
                return Content(string.Empty);
            }

            if (!IsLoggedIn())
            {
                return Content(string.Empty);
            }

            var model = new SA11YAccessiblityChecker();
            switch (_localizationService.GetDefaultLanguageIsoCode())
            {
                case "fr":
                    model.LanguageJS = "fr";
                    break;
                case "pl":
                    model.LanguageJS = "pl";
                    break;
                case "sv":
                    model.LanguageJS = "sv";
                    break;
                case "ua":
                    model.LanguageJS = "ua";
                    break;
            }
            return View(model);

        }
        private bool IsLoggedIn()
        {
            if (HttpContext.User==null)
            {
                return false;
            }
            if (HttpContext.User.Identities==null)
            {
                return false;
            }
            if (HttpContext.User.Identities == null)
            {
                return false;
            }
            if (!HttpContext.User.Identities.Any())
            {
                return false;
            }
            var authenticated= HttpContext.User.Identities.Where(x=>x.IsAuthenticated).ToList();
            if (authenticated==null || !authenticated.Any())
            {
                return false;
            }
            var backoffice=authenticated.Where(x=>x.IsBackOfficeAuthenticationType()).ToList();
            if (backoffice == null || !backoffice.Any())
            {
                return false;
            }
            return true;
            // This has returned false negative
            ////HttpContext.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
