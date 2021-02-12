using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table
{
    public class CreateTableOfDtoBuilder : IExecutableBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public CreateTableOfDtoBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public Type TypeOfDto { get; set; }

        public bool WithoutKeysAndIndexes { get; set; }

        /// <inheritdoc />
        public void Do()
        {
            var syntax = _context.SqlContext.SqlSyntax;
            var tableDefinition = DefinitionFactory.GetTableDefinition(TypeOfDto, syntax);

            ExecuteSql(syntax.Format(tableDefinition));
            if (WithoutKeysAndIndexes)
                return;

            ExecuteSql(syntax.FormatPrimaryKey(tableDefinition));
            foreach (var sql in syntax.Format(tableDefinition.ForeignKeys))
                ExecuteSql(sql);
            foreach (var sql in syntax.Format(tableDefinition.Indexes))
                ExecuteSql(sql);
        }

        private void ExecuteSql(string sql)
        {
            new ExecuteSqlStatementExpression(_context) { SqlStatement = sql }
                .Execute();
        }
    }
}
