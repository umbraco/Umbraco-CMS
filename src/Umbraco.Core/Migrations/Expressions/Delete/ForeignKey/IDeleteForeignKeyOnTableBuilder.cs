namespace Umbraco.Core.Migrations.Expressions.Delete.ForeignKey
{
    /// <summary>
    /// Builds a Delete Foreign Key expression.
    /// </summary>
    public interface IDeleteForeignKeyOnTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table of the foreign key, and executes.
        /// </summary>
        void OnTable(string foreignTableName);
    }
}
