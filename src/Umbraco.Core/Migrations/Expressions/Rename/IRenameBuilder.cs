using Umbraco.Core.Migrations.Expressions.Rename.Column;
using Umbraco.Core.Migrations.Expressions.Rename.Table;

namespace Umbraco.Core.Migrations.Expressions.Rename
{
    /// <summary>
    /// Builds a Rename expression.
    /// </summary>
    public interface IRenameBuilder : IFluentBuilder
    {
        /// <summary>
        /// Builds a Rename Table expression.
        /// </summary>
        IRenameTableBuilder Table(string oldName);

        /// <summary>
        /// Builds a Rename Column expression.
        /// </summary>
        IRenameColumnBuilder Column(string oldName);
    }
}
