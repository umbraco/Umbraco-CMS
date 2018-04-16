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
        /// Specifies the table to rename.
        /// </summary>
        IRenameTableBuilder Table(string oldName);

        /// <summary>
        /// Specifies the column to rename.
        /// </summary>
        IRenameColumnBuilder Column(string oldName);
    }
}
