using System;
using Umbraco.Core;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Umbraco.Extensions
{
    public static class LinkGeneratorExtensions
    {
        /// <summary>
        /// Return the back office url if the back office is installed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBackOfficeUrl(this LinkGenerator linkGenerator)
        {

            Type backOfficeControllerType;
            try
            {
                backOfficeControllerType = Assembly.Load("Umbraco.Web.BackOffice")?.GetType("Umbraco.Web.BackOffice.Controllers.BackOfficeController");
                if (backOfficeControllerType == null) return "/"; // this would indicate that the installer is installed without the back office
            }
            catch (Exception)
            {
                return "/"; // this would indicate that the installer is installed without the back office
            }
            
            return linkGenerator.GetPathByAction("Default", ControllerExtensions.GetControllerName(backOfficeControllerType), new { area = Constants.Web.Mvc.BackOfficeArea });
        }
    }
}
