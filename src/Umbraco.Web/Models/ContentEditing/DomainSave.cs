namespace Umbraco.Web.Models.ContentEditing
{
    public class DomainSave
    {
        public bool Valid { get; set; }
        public int NodeId { get; set; }
        public int Language { get; set; }
        public DomainDisplay[] Domains { get; set; }
    }
}
