using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    [ContentType("home", AllowedChildContentTypes = new[] { typeof(ContentPage) })]
    public class Home : ContentTypeBase
    {
        [PropertyType(typeof(TextboxPropertyEditor))]
        public string SiteName { get; set; }

        [Alias("umbSiteDescription", Name = "Site Description")] // ignored by the mapper at the moment
        [PropertyType(typeof(MultipleTextStringPropertyEditor))]
        public string SiteDescription { get; set; }
    }
}