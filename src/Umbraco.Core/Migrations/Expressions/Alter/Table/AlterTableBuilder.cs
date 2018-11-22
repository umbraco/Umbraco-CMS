using System.Data;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Alter.Expressions;
using Umbraco.Core.Migrations.Expressions.Common.Expressions;
using Umbraco.Core.Migrations.Expressions.Create.Expressions;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Alter.Table
{
    public class AlterTableBuilder : ExpressionBuilderBase<AlterTableExpression, IAlterTableColumnOptionBuilder>,
                                               IAlterTableColumnTypeBuilder,
                                               IAlterTableColumnOptionForeignKeyCascadeBuilder
    {
        private readonly IMigrationContext _context;

        public AlterTableBuilder(IMigrationContext context, AlterTableExpression expression)
            : base(expression)
        {
            _context = context;
        }

        public void Do() => Expression.Execute();

        public ColumnDefinition CurrentColumn { get; set; }

        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        public IAlterTableColumnOptionBuilder WithDefault(SystemMethods method)
        {
            CurrentColumn.DefaultValue = method;
            return this;
        }

        public IAlterTableColumnOptionBuilder WithDefaultValue(object value)
        {
            if (CurrentColumn.ModificationType == ModificationType.Alter)
            {
                var dc = new AlterDefaultConstraintExpression(_context)
                             {
                                 TableName = Expression.TableName,
                                 ColumnName = CurrentColumn.Name,
                                 DefaultValue = value
                             };

                Expression.Expressions.Add(dc);
            }

            CurrentColumn.DefaultValue = value;
            return this;
        }

        public IAlterTableColumnOptionBuilder Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public IAlterTableColumnOptionBuilder Indexed()
        {
            return Indexed(null);
        }

        public IAlterTableColumnOptionBuilder Indexed(string indexName)
        {
            CurrentColumn.IsIndexed = true;

            var index = new CreateIndexExpression(_context, new IndexDefinition
            {
                Name = indexName,
                TableName = Expression.TableName
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = CurrentColumn.Name
                                        });

            Expression.Expressions.Add(index);

            return this;
        }

        public IAlterTableColumnOptionBuilder PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;

            // see notes in CreateTableBuilder
            if (Expression.DatabaseType.IsMySql() == false)
            {
                var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
                {
                    Constraint =
                    {
                        TableName = Expression.TableName,
                        Columns = new[] { CurrentColumn.Name }
                    }
                };
                Expression.Expressions.Add(expression);
            }

            return this;
        }

        public IAlterTableColumnOptionBuilder PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;

            // see notes in CreateTableBuilder
            if (Expression.DatabaseType.IsMySql() == false)
            {
                var expression = new CreateConstraintExpression(_context, ConstraintType.PrimaryKey)
                {
                    Constraint =
                    {
                        ConstraintName = primaryKeyName,
                        TableName = Expression.TableName,
                        Columns = new[] { CurrentColumn.Name }
                    }
                };
                Expression.Expressions.Add(expression);
            }

            return this;
        }

        public IAlterTableColumnOptionBuilder Nullable()
        {
            CurrentColumn.IsNullable = true;
            return this;
        }

        public IAlterTableColumnOptionBuilder NotNullable()
        {
            CurrentColumn.IsNullable = false;
            return this;
        }

        public IAlterTableColumnOptionBuilder Unique()
        {
            return Unique(null);
        }

        public IAlterTableColumnOptionBuilder Unique(string indexName)
        {
            CurrentColumn.IsUnique = true;

            var index = new CreateIndexExpression(_context, new IndexDefinition
            {
                Name = indexName,
                TableName = Expression.TableName,               
                IndexType = IndexTypes.UniqueNonClustered
            });

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = CurrentColumn.Name
                                        });

            Expression.Expressions.Add(index);

            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(string foreignKeyName, string primaryTableName,
                                                                         string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey(string foreignKeyName, string primaryTableSchema,
                                                                         string primaryTableName, string primaryColumnName)
        {
            CurrentColumn.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression(_context, new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = primaryTableName,
                PrimaryTableSchema = primaryTableSchema,
                ForeignTable = Expression.TableName
            });

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);

            Expression.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                           string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder ReferencedBy(string foreignKeyName, string foreignTableSchema,
                                                                           string foreignTableName, string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression(_context, new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = Expression.TableName,
                ForeignTable = foreignTableName,
                ForeignTableSchema = foreignTableSchema
            });

            fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            Expression.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public IAlterTableColumnTypeBuilder AddColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Create };
            var createColumn = new CreateColumnExpression(_context)
                                   {
                                       Column = column,
                                       TableName = Expression.TableName
                                   };

            CurrentColumn = column;

            Expression.Expressions.Add(createColumn);
            return this;
        }

        public IAlterTableColumnTypeBuilder AlterColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ModificationType.Alter };
            var alterColumn = new AlterColumnExpression(_context)
            {
                Column = column,
                TableName = Expression.TableName
            };

            CurrentColumn = column;

            Expression.Expressions.Add(alterColumn);
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterTableColumnOptionForeignKeyCascadeBuilder OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterTableColumnOptionBuilder OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }
    }
}
