using System.Runtime.Remoting.Contexts;
using System.Web.Http.Controllers;

namespace Umbraco.Web.WebApi
{
    internal static class ControllerContextExtensions
    {
        /// <summary>
        /// Sets the JSON GUID format to not have hyphens
        /// </summary>
        /// <param name="controllerContext"></param>
        internal static void SetOutgoingNoHyphenGuidFormat(this HttpControllerContext controllerContext)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new GuidNoHyphenConverter());
        }


        /// <summary>
        /// Sets the JSON datetime format to be a custom one
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="format"></param>
        internal static void SetOutgoingDateTimeFormat(this HttpControllerContext controllerContext, string format)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new CustomDateTimeConvertor(format));
        }

        /// <summary>
        /// Sets the JSON datetime format to be our regular iso standard
        /// </summary>
        internal static void SetOutgoingDateTimeFormat(this HttpControllerContext controllerContext)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new CustomDateTimeConvertor("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Removes the xml formatter so it only outputs json
        /// </summary>
        /// <param name="controllerContext"></param>
        internal static void EnsureJsonOutputOnly(this HttpControllerContext controllerContext)
        {
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }
    }
}