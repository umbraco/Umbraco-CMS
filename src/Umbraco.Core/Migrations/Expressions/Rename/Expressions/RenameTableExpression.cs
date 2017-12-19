using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Rename.Expressions
{
    /// <summary>
    /// Represents a Rename Table expression.
    /// </summary>
    public class RenameTableExpression : MigrationExpressionBase
    {
        public RenameTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        public virtual string OldName { get; set; }

        /// <summary>
        /// Gets or sets the target name.
        /// </summary>
        public virtual string NewName { get; set; }

        /// <inheritdoc />
        public override string ToString() // fixme kill
            => GetSql();

        /// <inheritdoc />
        protected override string GetSql()
        {
            return IsExpressionSupported() == false
                ? string.Empty
                : SqlSyntax.FormatTableRename(OldName, NewName);
        }
    }
}
