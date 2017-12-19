namespace Umbraco.Core.Migrations.Expressions.Rename.Column
{
    /// <summary>
    /// Builds a Rename Column expression.
    /// </summary>
    public interface IRenameColumnToBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the new name of the column and executes.
        /// </summary>
        void To(string name);
    }
}
