namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Constraint
{
    public interface ICreateConstraintColumnsSyntax : IFluentSyntax
    {
        void Column(string columnName);
        void Columns(string[] columnNames);
    }
}