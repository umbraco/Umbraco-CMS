namespace Umbraco.Web.Editors
{
    using Umbraco.Web.Editors.TemplateQuery;

    public class SortExpression
    {
        public PropertyModel Property { get; set; }

        public string SortDirection { get; set; }
    }
}