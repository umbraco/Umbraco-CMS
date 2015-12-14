using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Table
{
    public class AlterTableBuilder : ExpressionBuilder<AlterTableExpression, IAlterTableColumnOptionSyntax>,
                                               IAlterTableColumnTypeSyntax,
                                               IAlterTableColumnOptionForeignKeyCascadeSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public AlterTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, AlterTableExpression expression)
            : base(expression)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public ColumnDefinition CurrentColumn { get; set; }

        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        public IAlterTableColumnOptionSyntax WithDefault(SystemMethods method)
        {
            CurrentColumn.DefaultValue = method;
            return this;
        }

        public IAlterTableColumnOptionSyntax WithDefaultValue(object value)
        {
            if (CurrentColumn.ModificationType == ModificationType.Alter)
            {
                var dc = new AlterDefaultConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax)
                             {
                                 TableName = Expression.TableName,
                                 SchemaName = Expression.SchemaName,
                                 ColumnName = CurrentColumn.Name,
                                 DefaultValue = value
                             };

                _context.Expressions.Add(dc);
            }

            CurrentColumn.DefaultValue = value;
            return this;
        }

        public IAlterTableColumnOptionSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public IAlterTableColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public IAlterTableColumnOptionSyntax Indexed(string indexName)
        {
            CurrentColumn.IsIndexed = true;

            var index = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = CurrentColumn.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterTableColumnOptionSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;
            return this;
        }

        public IAlterTableColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public IAlterTableColumnOptionSyntax Nullable()
        {
            CurrentColumn.IsNullable = true;
            return this;
        }

        public IAlterTableColumnOptionSyntax NotNullable()
        {
            CurrentColumn.IsNullable = false;
            return this;
        }

        public IAlterTableColumnOptionSyntax Unique()
        {
            return Unique(null);
        }

        public IAlterTableColumnOptionSyntax Unique(string indexName)
        {
            CurrentColumn.IsUnique = true;

            var index = new CreateIndexExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new IndexDefinition
            {
                Name = indexName,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName,
                IsUnique = true
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = CurrentColumn.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                         string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
                                                                         string primaryTableName, string primaryColumnName)
        {
            CurrentColumn.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = primaryTableName,
                PrimaryTableSchema = primaryTableSchema,
                ForeignTable = Expression.TableName,
                ForeignTableSchema = Expression.SchemaName
            });

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                           string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
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

            fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterTableColumnTypeSyntax AddColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Create };
            var createColumn = new CreateColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax)
                                   {
                                       Column = column,
                                       SchemaName = Expression.SchemaName,
                                       TableName = Expression.TableName
                                   };

            CurrentColumn = column;

            _context.Expressions.Add(createColumn);
            return this;
        }

        public IAlterTableColumnTypeSyntax AlterColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Alter };
            var alterColumn = new AlterColumnExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax)
            {
                Column = column,
                SchemaName = Expression.SchemaName,
                TableName = Expression.TableName
            };

            CurrentColumn = column;

            _context.Expressions.Add(alterColumn);
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterTableColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }
    }
}