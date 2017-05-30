using System;
using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter
{
    public class AlterSyntaxBuilder : IAlterSyntaxBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public AlterSyntaxBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public IAlterTableSyntax Table(string tableName)
        {
            var expression = new AlterTableExpression(_context, _supportedDatabaseTypes) { TableName = tableName };
            //_context.Expressions.Add(expression);
            return new AlterTableBuilder(_context, _supportedDatabaseTypes, expression);
        }

        /// <summary>
        /// The problem with this is that only under particular circumstances is the expression added to the context
        /// so you wouldn't actually know if you are using it correctly or not and chances are you are not and therefore
        /// the statement won't even execute whereas using the IAlterTableSyntax to modify a column is guaranteed to add
        /// the expression to the context.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [Obsolete("Use the IAlterTableSyntax to modify a column instead, this will be removed in future versions")]
        public IAlterColumnSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression(_context, _supportedDatabaseTypes) {Column = {Name = columnName}};
            //_context.Expressions.Add(expression);
            return new AlterColumnBuilder(_context, _supportedDatabaseTypes, expression);
        }
    }
}