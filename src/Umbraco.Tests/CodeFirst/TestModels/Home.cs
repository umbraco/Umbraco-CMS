using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.editorControls.textfield;
using umbraco.editorControls.textfieldmultiple;

namespace Umbraco.Tests.CodeFirst.TestModels
{
    [ContentType("home", AllowedChildContentTypes = new[] { typeof(ContentPage) })]
    public class Home : ContentTypeBase
    {
        [PropertyType(typeof(TextFieldDataType))]
        public string SiteName { get; set; }

        [Alias("umbSiteDescription", Name = "Site Description")] // ignored by the mapper at the moment
        [PropertyType(typeof(textfieldMultipleDataType))]
        public string SiteDescription { get; set; }
    }
}