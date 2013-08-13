using System.Web.Http.Filters;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Sets the json outgoing/serialized datetime format
    /// </summary>
    internal sealed class OutgoingDateTimeFormatAttribute : ActionFilterAttribute
    {
        private readonly string _format;

        /// <summary>
        /// Specify a custom format
        /// </summary>
        /// <param name="format"></param>
        public OutgoingDateTimeFormatAttribute(string format)
        {
            Mandate.ParameterNotNullOrEmpty(format, "format");
            _format = format;
        }

        /// <summary>
        /// Will use the standard ISO format
        /// </summary>
        public OutgoingDateTimeFormatAttribute()
        {
            
        }

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            if (_format.IsNullOrWhiteSpace())
            {
                actionContext.ControllerContext.SetOutgoingDateTimeFormat();
            }
            else
            {
                actionContext.ControllerContext.SetOutgoingDateTimeFormat(_format);
            }
            
        }
    }
}