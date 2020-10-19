﻿using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Security;
using System.Globalization;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Applied to all Umbraco controllers to ensure the thread culture is set to the culture assigned to the back office identity
    /// </summary>
    public class BackOfficeCultureFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var culture = context.HttpContext.User.Identity.GetCulture();
            if (culture != null)
            {
                SetCurrentThreadCulture(culture);
            }
        }

        private static void SetCurrentThreadCulture(CultureInfo culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }


}
