using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{

    /// <summary>
    /// Allows an Action to execute with an arbitrary number of QueryStrings
    /// </summary>
    /// <remarks>
    /// Just like you can POST an arbitrary number of parameters to an Action, you can't GET an arbitrary number
    /// but this will allow you to do it
    /// 
    /// http://stackoverflow.com/questions/488061/passing-multiple-parameters-to-controller-in-asp-net-mvc-also-generating-on-the
    /// </remarks>
    public class QueryStringFilterAttribute : ActionFilterAttribute
    {
        public string ParameterName { get; private set; }

        public QueryStringFilterAttribute(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("ParameterName is required.");
            ParameterName = parameterName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nonNullKeys = filterContext.HttpContext.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x));
            var vals = nonNullKeys.ToDictionary(q => q, q => (object)filterContext.HttpContext.Request.QueryString[q]);
            var qs = ToFormCollection(vals);

            filterContext.ActionParameters[ParameterName] = qs;

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Converts a dictionary to a FormCollection
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static FormCollection ToFormCollection(IDictionary<string, object> d)
        {
            var n = new FormCollection();
            foreach (var i in d)
            {
                n.Add(i.Key, Convert.ToString(i.Value));
            }
            return n;
        }

    }

}
