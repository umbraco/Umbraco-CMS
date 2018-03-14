namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// The model representing Previewing of a content item from the back office
    /// </summary>
    public class BackOfficePreview
    {
        public string PreviewExtendedHeaderView { get; set; }
        //TODO: We could potentially have a 'footer' view
        public bool DisableDevicePreview { get; set; }
    }
}
