namespace Umbraco.Web.Editors
{
    public interface ISortExpression
    {
        string FieldName { get; set; }

        string SortDirection { get; set; }
    }

    public class SortExpression : ISortExpression
    {
        public string FieldName { get; set; }

        public string SortDirection { get; set; }
    }
}