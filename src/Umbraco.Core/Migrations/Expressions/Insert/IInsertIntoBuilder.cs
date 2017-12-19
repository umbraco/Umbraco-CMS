namespace Umbraco.Core.Migrations.Expressions.Insert
{
    /// <summary>
    /// Builds an Insert Into expression.
    /// </summary>
    public interface IInsertIntoBuilder : IFluentBuilder
    {
        /// <summary>
        /// Enables identity insert.
        /// </summary>
        IInsertIntoBuilder EnableIdentityInsert();

        /// <summary>
        /// Specifies a row to be inserted.
        /// </summary>
        IInsertIntoBuilder Row(object dataAsAnonymousType);

        /// <summary>
        /// Executes.
        /// </summary>
        void Execute();
    }
}
