using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// MVC handler to facilitate the TemplateRenderer. This handler can execute an MVC request and return it as a string.
    /// 
    /// Original:
    /// 
    /// This handler also used to intercept creation of controllers and store it for later use.
    /// This was needed for the 'return CurrentUmbracoPage()' surface controller functionality 
    /// because it needs to send data back to the page controller.
    /// 
    /// The creation of this controller has been moved to the UmbracoPageResult class which will create a controller when needed.
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