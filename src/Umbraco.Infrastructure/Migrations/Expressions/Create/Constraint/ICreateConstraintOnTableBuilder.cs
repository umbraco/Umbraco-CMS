namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public interface ICreateConstraintOnTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table name.
        /// </summary>
        ICreateConstraintColumnsBuilder OnTable(string tableName);
    }
}
