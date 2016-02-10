namespace Umbraco.Web.Models.ContentEditing
{
    public class GetAvailableCompositionsFilter
    {
        public int ContentTypeId { get; set; }
        public string[] FilterPropertyTypes { get; set; }
        public string[] FilterContentTypes { get; set; }
    }
}