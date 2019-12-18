namespace Umbraco.Core.Migrations.Expressions.Rename.Expressions
{
    public class RenameColumnExpression : MigrationExpressionBase
    {
        public RenameColumnExpression(IMigrationContext context)
            : base(context)
        { }

        public virtual string TableName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string Process(IMigrationContext context)
            => GetSql();

        /// <inheritdoc />
        protected override string GetSql()
        {
            return SqlSyntax.FormatColumnRename(TableName, OldName, NewName);
        }
    }
}
