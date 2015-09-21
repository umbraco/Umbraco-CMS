using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Table
{
    public class CreateTableBuilder : ExpressionBuilder<CreateTableExpression, ICreateTableColumnOptionSyntax>,
                                                ICreateTableWithColumnSyntax,
                                                ICreateTableColumnAsTypeSyntax,
                                                ICreateTableColumnOptionForeignKeyCascadeSyntax
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public CreateTableBuilder(IMigrationContext context, DatabaseProviders[] databaseProviders, CreateTableExpression expression)
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

        public ICreateTableColumnAsTypeSyntax WithColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, TableName = Expression.TableName, ModificationType = ModificationType.Create };
            Expression.Columns.Add(column);
            CurrentColumn = column;
            return this;
        }

        public ICreateTableColumnOptionSyntax WithDefault(SystemMethods method)
        {
            CurrentColumn.DefaultValue = method;
            return this;
        }

        public ICreateTableColumnOptionSyntax WithDefaultValue(object value)
        {
            CurrentColumn.DefaultValue = value;
            return this;
        }

        public ICreateTableColumnOptionSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public ICreateTableColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public ICreateTableColumnOptionSyntax Indexed(string indexName)
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

        public ICreateTableColumnOptionSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;

            //For MySQL, the PK will be created WITH the create table expression, however for 
            // SQL Server, the PK get's created in a different Alter table expression afterwords.
            // MySQL will choke if the same constraint is again added afterword
            // TODO: This is a super hack, I'd rather not add another property like 'CreatesPkInCreateTableDefinition' to check
            // for this, but I don't see another way around. MySQL doesn't support checking for a constraint before creating
            // it... except in a very strange way but it doesn't actually provider error feedback if it doesn't work so we cannot use
            // it.  For now, this is what I'm doing
            if (Expression.CurrentDatabaseProvider != DatabaseProviders.MySql)
            {
                var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, ConstraintType.PrimaryKey)
                {
                    Constraint =
                {
                    TableName = CurrentColumn.TableName,
                    Columns = new[] { CurrentColumn.Name }
                }
                };
                _context.Expressions.Add(expression);
            }

            return this;
        }

        public ICreateTableColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;

            //For MySQL, the PK will be created WITH the create table expression, however for 
            // SQL Server, the PK get's created in a different Alter table expression afterwords.
            // MySQL will choke if the same constraint is again added afterword
            // TODO: This is a super hack, I'd rather not add another property like 'CreatesPkInCreateTableDefinition' to check
            // for this, but I don't see another way around. MySQL doesn't support checking for a constraint before creating
            // it... except in a very strange way but it doesn't actually provider error feedback if it doesn't work so we cannot use
            // it.  For now, this is what I'm doing

            if (Expression.CurrentDatabaseProvider != DatabaseProviders.MySql)
            {
                var expression = new CreateConstraintExpression(_context.CurrentDatabaseProvider, _databaseProviders, Expression.SqlSyntax, ConstraintType.PrimaryKey)
                {
                    Constraint =
                {
                    ConstraintName = primaryKeyName,
                    TableName = CurrentColumn.TableName,
                    Columns = new[] { CurrentColumn.Name }
                }
                };
                _context.Expressions.Add(expression);
            }
            
            return this;
        }

        public ICreateTableColumnOptionSyntax Nullable()
        {
            CurrentColumn.IsNullable = true;
            return this;
        }

        public ICreateTableColumnOptionSyntax NotNullable()
        {
            CurrentColumn.IsNullable = false;
            return this;
        }

        public ICreateTableColumnOptionSyntax Unique()
        {
            return Unique(null);
        }

        public ICreateTableColumnOptionSyntax Unique(string indexName)
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

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                          string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
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

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                            string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
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

        public ICreateTableColumnOptionForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateTableColumnOptionForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public ICreateTableColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }
    }
}