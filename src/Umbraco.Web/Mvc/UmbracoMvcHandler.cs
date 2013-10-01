using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Mvc handler class to intercept creation of controller and store it for later use.
    /// This means we create two instances of the same controller to support some features later on.
    /// 
    /// The alternate option for this is to completely rewrite all MvcHandler methods.
    /// 
    /// This is currently needed for the 'return CurrentUmbracoPage()' surface controller functionality 
    /// because it needs to send data back to the page controller.
    /// </summary>
    internal class UmbracoMvcHandler : MvcHandler
    {
        public UmbracoMvcHandler(RequestContext requestContext)
            : base(requestContext)
        {
        }
       
        /// <summary>
        /// This is used internally purely to render an Umbraco MVC template to string and shouldn't be used for anything else.
        /// </summary>
        internal void ExecuteUmbracoRequest()
        {
            base.ProcessRequest(RequestContext.HttpContext);
        }
    }
}