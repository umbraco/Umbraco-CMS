namespace Umbraco.Core.Migrations.Expressions.Rename.Table
{
    /// <summary>
    /// Builds a Rename Table expression.
    /// </summary>
    public interface IRenameTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the new name of the table and executes.
        /// </summary>
        void To(string name);
    }
}
