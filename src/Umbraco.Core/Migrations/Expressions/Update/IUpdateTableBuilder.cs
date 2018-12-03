namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Builds an Update expression.
    /// </summary>
    public interface IUpdateTableBuilder
    {
        /// <summary>
        /// Specifies the data.
        /// </summary>
        IUpdateWhereBuilder Set(object dataAsAnonymousType);
    }
}
