namespace Umbraco.Core.Migrations.Expressions.Delete.ForeignKey
{
    /// <summary>
    /// Builds a Delete Foreign Key expression.
    /// </summary>
    public interface IDeleteForeignKeyToTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the target table of the foreign key.
        /// </summary>
        IDeleteForeignKeyPrimaryColumnBuilder ToTable(string table);
    }
}
