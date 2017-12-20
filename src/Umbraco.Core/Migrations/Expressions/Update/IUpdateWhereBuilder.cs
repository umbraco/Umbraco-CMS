using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Builds an Update expression.
    /// </summary>
    public interface IUpdateWhereBuilder
    {
        /// <summary>
        /// Specifies rows to update.
        /// </summary>
        IExecutableBuilder Where(object dataAsAnonymousType);

        /// <summary>
        /// Specifies that all rows must be updated.
        /// </summary>
        IExecutableBuilder AllRows();
    }
}
