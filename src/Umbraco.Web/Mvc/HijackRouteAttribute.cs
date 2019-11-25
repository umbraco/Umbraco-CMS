using System;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used to define a controller action as being
    /// an entry point for a hijacked route
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HijackRouteAttribute : Attribute
    {
        public string ContentTypeAlias { get; set; }
        public string TemplateAlias { get; set; }

        public HijackRouteAttribute(string contentTypeAlias)
        {
            ContentTypeAlias = contentTypeAlias;
            TemplateAlias = null;
        }

        public HijackRouteAttribute(string contentTypeAlias, string templateAlias)
        {
            ContentTypeAlias = contentTypeAlias;
            TemplateAlias = templateAlias;
        }
    }

}
