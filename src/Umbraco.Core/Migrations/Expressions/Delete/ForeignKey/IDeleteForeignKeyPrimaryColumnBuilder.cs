namespace Umbraco.Core.Migrations.Expressions.Delete.ForeignKey
{
    /// <summary>
    /// Builds a Delete Foreign Key expression.
    /// </summary>
    public interface IDeleteForeignKeyPrimaryColumnBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the target primary column, and executes.
        /// </summary>
        void PrimaryColumn(string column);

        /// <summary>
        /// Specifies the target primary columns, and executes.
        /// </summary>
        void PrimaryColumns(params string[] columns);
    }
}
