using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// An exception filter checking if we get a <see cref="ModelBindingException"/> in which case it returns the html to auto refresh the page
    /// </summary>
    internal class ModelBindingExceptionFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled && filterContext.Exception is ModelBindingException)
            {
                filterContext.Result = new ContentResult
                {
                    Content = "<html><head><meta http-equiv=\"refresh\" content=\"3\" /></head><body><p>Loading page...</p></body></html>",
                    ContentType = "text/html"
                };
                filterContext.ExceptionHandled = true;
            }
        }
    }
}
