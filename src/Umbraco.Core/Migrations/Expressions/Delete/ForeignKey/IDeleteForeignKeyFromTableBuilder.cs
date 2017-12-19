namespace Umbraco.Core.Migrations.Expressions.Delete.ForeignKey
{
    /// <summary>
    /// Builds a Delete Foreign Key expression.
    /// </summary>
    public interface IDeleteForeignKeyFromTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the source table of the foreign key.
        /// </summary>
        IDeleteForeignKeyForeignColumnBuilder FromTable(string foreignTableName);
    }
}
