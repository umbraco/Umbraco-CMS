namespace Umbraco.Web.Models.ContentEditing
{
    public class DomainDisplay
    {
        public DomainDisplay(string name, int lang)
        {
            Name = name;
            Lang = lang;
        }

        public string Name { get; }
        public int Lang { get; }
        public bool Duplicate { get; set; }
        public string Other { get; set; }
    }
}
