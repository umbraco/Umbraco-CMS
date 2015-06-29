using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteForeignKeyExpression : MigrationExpressionBase
    {
        [Obsolete("Use the other constructors specifying an ILogger instead")]
        public DeleteForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        [Obsolete("Use the other constructors specifying an ILogger instead")]
        public DeleteForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : base(current, databaseProviders)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public DeleteForeignKeyExpression(ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public DeleteForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {   
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            if (ForeignKey.ForeignTable == null)
                throw new ArgumentNullException("Table name not specified, ensure you have appended the OnTable extension. Format should be Delete.ForeignKey(KeyName).OnTable(TableName)");

            if (CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                //MySql naming "convention" for foreignkeys, which aren't explicitly named
                if (string.IsNullOrEmpty(ForeignKey.Name))
                    ForeignKey.Name = string.Format("{0}_ibfk_1", ForeignKey.ForeignTable.ToLower());

                return string.Format(SqlSyntax.DeleteConstraint,
                                 SqlSyntax.GetQuotedTableName(ForeignKey.ForeignTable),
                                 "FOREIGN KEY",
                                 SqlSyntax.GetQuotedName(ForeignKey.Name));
            }

            if (string.IsNullOrEmpty(ForeignKey.Name))
            {
                ForeignKey.Name = string.Format("FK_{0}_{1}_{2}", ForeignKey.ForeignTable, ForeignKey.PrimaryTable, ForeignKey.PrimaryColumns.First());
            }

            return string.Format(SqlSyntax.DeleteConstraint,
                                 SqlSyntax.GetQuotedTableName(ForeignKey.ForeignTable),
                                 SqlSyntax.GetQuotedName(ForeignKey.Name));
        }
    }
}