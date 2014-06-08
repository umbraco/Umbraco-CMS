namespace Umbraco.Web.Editors
{
    public interface IQueryCondition
    {
        string FieldName { get; set; }
        
        IOperathorTerm Term { get; set; }

        string ConstraintValue { get; set; }
    }
}