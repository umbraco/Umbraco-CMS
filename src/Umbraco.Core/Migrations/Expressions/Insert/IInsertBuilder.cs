namespace Umbraco.Core.Migrations.Expressions.Insert
{
    /// <summary>
    /// Builds an Insert expression.
    /// </summary>
    public interface IInsertBuilder : IFluentBuilder
    {
        /// <summary>
        /// Builds an Insert Into expression.
        /// </summary>
        IInsertIntoBuilder IntoTable(string tableName);
    }
}
