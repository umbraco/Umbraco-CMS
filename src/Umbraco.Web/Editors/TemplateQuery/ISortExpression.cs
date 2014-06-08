namespace Umbraco.Web.Editors
{
    public interface ISortExpression
    {
        string FieldName { get; set; }

        string SortDirection { get; set; }
    }
}