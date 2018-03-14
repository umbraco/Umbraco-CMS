namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The model representing Previewing of a content item from the back office
    /// </summary>
    public class BackOfficePreview
    {
        public string PreviewExtendedView { get; set; }
        public bool DisableDevicePreview { get; set; }
    }
}
