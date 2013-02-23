using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.WebServices
{
    public class ExamineManagementController : UmbracoAuthorizedController
    {
        /// <summary>
        /// Get the details
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View("~" + GlobalSettings.Path.EnsureEndsWith('/').EnsureStartsWith('/') + "Views/ExamineManagement/ExamineDetails.cshtml");
        }

    }
}
