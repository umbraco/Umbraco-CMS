namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Builds an Update expression.
    /// </summary>
    public interface IUpdateBuilder : IFluentBuilder
    {
        /// <summary>
        /// Builds an Update Table expression.
        /// </summary>
        IUpdateTableBuilder Table(string tableName);
    }
}
