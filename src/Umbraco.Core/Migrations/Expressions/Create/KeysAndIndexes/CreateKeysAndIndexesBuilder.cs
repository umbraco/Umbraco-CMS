using System;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Migrations.Expressions.Create.KeysAndIndexes
{
    public class CreateKeysAndIndexesBuilder : IExecutableBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public CreateKeysAndIndexesBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public Type TypeOfDto { get; set; }

        /// <inheritdoc />
        public void Do()
        {
            var syntax = _context.SqlContext.SqlSyntax;
            var tableDefinition = DefinitionFactory.GetTableDefinition(TypeOfDto, syntax);

            // fixme
            //var exists = _context.SqlContext.SqlSyntax.DoesTableExist(_context.Database, tableDefinition.Name);
            //if (!exists) return; // no point creating keys and indexes on non-existing table

            //// problem: this is going to create the keys and constraints,
            //// using the 'latest' table names - but if we do it in a migration,
            //// and later on rename a table, we are going to try to create keys
            //// and constraints referencing the new table name, ie something
            //// that does not exist yet!
            ////
            //// fixme should we also NOT create those that already exist?
            // or, completely prevent removing ALL keys?
            // but even on 1 table, if we remove keys, we may not be able to recreate them all
            // and then... Create.Table<Foo> is always going to fail? need to use it with COPIES of the DTO class!
            // so that it tries to create things that make sense _at the time it runs_
            //
            // eg the new PropertyData points to new DataType that does not exist yet
            //  because... we should have kept a PropertyData that points to... a DataType = TWO dtos!
            //  so basically, keep a 'graveyard' of old dtos? for migrations only?

            ExecuteSql(syntax.FormatPrimaryKey(tableDefinition));
            foreach (var sql in syntax.Format(tableDefinition.Indexes))
                ExecuteSql(sql);
            foreach (var sql in syntax.Format(tableDefinition.ForeignKeys))
                ExecuteSql(sql);
        }

        private void ExecuteSql(string sql)
        {
            new ExecuteSqlStatementExpression(_context) { SqlStatement = sql }
                .Execute();
        }
    }
}
