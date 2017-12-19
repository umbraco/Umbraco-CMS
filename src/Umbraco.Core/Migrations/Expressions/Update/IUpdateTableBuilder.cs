namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Builds an Update Table expression.
    /// </summary>
    public interface IUpdateTableBuilder
    {
        /// <summary>
        /// Specifies the data.
        /// </summary>
        IUpdateWhereBuilder Set(object dataAsAnonymousType);
    }
}
