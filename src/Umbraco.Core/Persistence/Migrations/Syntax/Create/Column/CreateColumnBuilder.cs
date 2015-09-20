using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Column
{
    public class CreateColumnBuilder : ExpressionBuilder<CreateColumnExpression, ICreateColumnOptionSyntax>,
                                             ICreateColumnOnTableSyntax,
                                             ICreateColumnTypeSyntax,
                                             ICreateColumnOptionForeignKeyCascadeSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public CreateColumnBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, CreateColumnExpression expression)
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

        public ICreateColumnTypeSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        public ICreateColumnOptionSyntax WithDefault(SystemMethods method)
        {
            Expression.Column.DefaultValue = method;
            return this;
        }

        public ICreateColumnOptionSyntax WithDefaultValue(object value)
        {
            Expression.Column.DefaultValue = value;
            return this;
        }

        public ICreateColumnOptionSyntax Identity()
        {
            return Indexed(null);
        }

        public ICreateColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public ICreateColumnOptionSyntax Indexed(string indexName)
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

        public ICreateColumnOptionSyntax PrimaryKey()
        {
            Expression.Column.IsPrimaryKey = true;
            return this;
        }

        public ICreateColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            Expression.Column.IsPrimaryKey = true;
            Expression.Column.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public ICreateColumnOptionSyntax Nullable()
        {
            Expression.Column.IsNullable = true;
            return this;
        }

        public ICreateColumnOptionSyntax NotNullable()
        {
            Expression.Column.IsNullable = false;
            return this;
        }

        public ICreateColumnOptionSyntax Unique()
        {
            return Unique(null);
        }

        public ICreateColumnOptionSyntax Unique(string indexName)
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

        public ICreateColumnOptionForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                     string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
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

        public ICreateColumnOptionForeignKeyCascadeSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                       string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
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

        public ICreateColumnOptionForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateColumnOptionForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public ICreateColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }
    }
}