using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Column
{
    public class AlterColumnBuilder : ExpressionBuilder<AlterColumnExpression, IAlterColumnOptionSyntax>,
                                            IAlterColumnSyntax,
                                            IAlterColumnTypeSyntax,
                                            IAlterColumnOptionForeignKeyCascadeSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public AlterColumnBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, AlterColumnExpression expression)
            : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        public override ColumnDefinition GetColumnForType()
        {
            return Expression.Column;
        }

        public IAlterColumnTypeSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        public IAlterColumnOptionSyntax WithDefault(SystemMethods method)
        {
            var dc = new AlterDefaultConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax)
                         {
                             TableName = Expression.TableName,
                             SchemaName = Expression.SchemaName,
                             ColumnName = Expression.Column.Name,
                             DefaultValue = method
                         };

            _context.Expressions.Add(dc);

            Expression.Column.DefaultValue = method;

            return this;
        }
        

        public IAlterColumnOptionSyntax WithDefaultValue(object value)
        {
            var dc = new AlterDefaultConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax)
                         {
                             TableName = Expression.TableName,
                             SchemaName = Expression.SchemaName,
                             ColumnName = Expression.Column.Name,
                             DefaultValue = value
                         };

            _context.Expressions.Add(dc);

            Expression.Column.DefaultValue = value;

            return this;
        }

        public IAlterColumnOptionSyntax Identity()
        {
            Expression.Column.IsIdentity = true;
            return this;
        }

        public IAlterColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public IAlterColumnOptionSyntax Indexed(string indexName)
        {
            Expression.Column.IsIndexed = true;

            var index = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = Expression.Column.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey()
        {
            Expression.Column.IsPrimaryKey = true;
            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            Expression.Column.IsPrimaryKey = true;
            Expression.Column.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public IAlterColumnOptionSyntax Nullable()
        {
            Expression.Column.IsNullable = true;
            return this;
        }

        public IAlterColumnOptionSyntax NotNullable()
        {
            Expression.Column.IsNullable = false;
            return this;
        }

        public IAlterColumnOptionSyntax Unique()
        {
            return Unique(null);
        }

        public IAlterColumnOptionSyntax Unique(string indexName)
        {
            Expression.Column.IsUnique = true;

            var index = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName,
                IsUnique = true
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = Expression.Column.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                    string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
                                                                    string primaryTableName, string primaryColumnName)
        {
            Expression.Column.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = primaryTableName,
                PrimaryTableSchema = primaryTableSchema,
                ForeignTable = Expression.TableName,
                ForeignTableSchema = Expression.SchemaName
            });

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(Expression.Column.Name);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                      string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
                                                                      string foreignTableName, string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = Expression.TableName,
                PrimaryTableSchema = Expression.SchemaName,
                ForeignTable = foreignTableName,
                ForeignTableSchema = foreignTableSchema
            });

            fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterColumnOptionForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }
    }
}