using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;
using umbraco.editorControls.textfieldmultiple;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    [ContentType("home", AllowedChildContentTypes = new[] { typeof(ContentPage) })]
    public class Home
    {
        [PropertyType(typeof(TextFieldDataType))]
        public string SiteName { get; set; }

        [Alias("umbSiteDescription", Name = "Site Description")]
        [PropertyType(typeof(textfieldMultipleDataType))]
        public string SiteDescription { get; set; }
    }
}