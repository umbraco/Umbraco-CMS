namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Builds an Update Table ... Where expression.
    /// </summary>
    public interface IUpdateWhereBuilder
    {
        /// <summary>
        /// Specifies rows to update, and executes.
        /// </summary>
        void Where(object dataAsAnonymousType);

        /// <summary>
        /// Specifies that all rows must be updated, and executes.
        /// </summary>
        void AllRows();
    }
}
