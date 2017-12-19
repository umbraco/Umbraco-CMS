namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public interface ICreateConstraintColumnsBuilder : IFluentBuilder
    {
        void Column(string columnName);
        void Columns(string[] columnNames);
    }
}
