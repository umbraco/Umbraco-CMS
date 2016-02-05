namespace Umbraco.Web.Models
{
    public class GetAvailableCompositionsFilter
    {
        public int ContentTypeId { get; set; }
        public string[] FilterPropertyTypes { get; set; }
        public string[] FilterContentTypes { get; set; }
    }
}