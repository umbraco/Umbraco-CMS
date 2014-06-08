namespace Umbraco.Web.Editors
{
    public interface ITemplateQueryResult
    {
        string Icon { get; set; }
        string Name { get; set; }
        
    }

    public class TemplateQueryResult : ITemplateQueryResult
    {
        public string Icon { get; set; }

        public string Name { get; set; }
    }
}