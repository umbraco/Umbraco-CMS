using System;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public interface IAlterSyntaxBuilder : IFluentSyntax
    {
        IAlterTableSyntax Table(string tableName);

        /// <summary>
        /// The problem with this is that only under particular circumstances is the expression added to the context
        /// so you wouldn't actually know if you are using it correctly or not and chances are you are not and therefore
        /// the statement won't even execute whereas using the IAlterTableSyntax to modify a column is guaranteed to add
        /// the expression to the context.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [Obsolete("Use the IAlterTableSyntax to modify a column instead, this will be removed in future versions")]
        IAlterColumnSyntax Column(string columnName);
    }
}