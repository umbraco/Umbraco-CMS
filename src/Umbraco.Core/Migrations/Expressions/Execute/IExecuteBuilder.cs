using System;

namespace Umbraco.Core.Migrations.Expressions.Execute
{
    /// <summary>
    /// Builds and executes an Sql statement.
    /// </summary>
    /// <remarks>Deals with multi-statements Sql.</remarks>
    public interface IExecuteBuilder : IFluentBuilder
    {
        /// <summary>
        /// Executes an Sql statement.
        /// </summary>
        void Sql(string sqlStatement);

        [Obsolete("kill.kill.kill")]
        void Code(Func<IMigrationContext, string> codeStatement);
    }
}
