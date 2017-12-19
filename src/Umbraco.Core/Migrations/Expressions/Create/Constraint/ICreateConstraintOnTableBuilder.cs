namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public interface ICreateConstraintOnTableBuilder : IFluentBuilder
    {
        ICreateConstraintColumnsBuilder OnTable(string tableName);
    }
}
