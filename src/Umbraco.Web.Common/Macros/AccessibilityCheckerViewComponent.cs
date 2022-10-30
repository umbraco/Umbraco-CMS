using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Umbraco.Cms.Core.Models.Preview;

namespace Umbraco.Cms.Web.Common.Macros
{
    public class AccessibilityCheckerViewComponent : ViewComponent
    {
        //private readonly ILocalizationService _localizationService;
        //private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        //public AccessibilityCheckerController(IUmbracoContextAccessor umbracoContextAccessor, ILocalizationService localizationService) : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService, serializer)
        public AccessibilityCheckerViewComponent()
        {
            //_localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            //_umbracoContextAccessor =
            //umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }
        [HttpGet]
        public IViewComponentResult Invoke()
        {
            //IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            //if (umbracoContext== null)
            //{
            //    return new EmptyResult();
            //}

            //if (!umbracoContext.InPreviewMode)
            //{
            //    return new EmptyResult();
            //}
            //var isPreview = Request.HasPreviewCookie()
            //    && !Request.IsBackOfficeRequest();

            //if (!isPreview)
            //{
            //    return Content(string.Empty);
            //}
            if (TempData.ContainsKey("CanvasDesigner"))
            {
                return Content(string.Empty);
            }

            var model = new SA11YAccessiblityChecker();
            //switch (_localizationService.GetDefaultLanguageIsoCode())
            //{
            //    case "fr":
            //        model.LanguageJS = "fr";
            //        break;
            //    case "pl":
            //        model.LanguageJS = "pl";
            //        break;
            //    case "sv":
            //        model.LanguageJS = "sv";
            //        break;
            //    case "ua":
            //        model.LanguageJS = "ua";
            //        break;
            //}
            return View(model);

        }
    }
}
