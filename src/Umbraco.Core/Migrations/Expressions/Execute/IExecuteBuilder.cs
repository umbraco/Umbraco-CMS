﻿using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations.Expressions.Execute
{
    /// <summary>
    /// Builds and executes an Sql statement.
    /// </summary>
    /// <remarks>Deals with multi-statements Sql.</remarks>
    public interface IExecuteBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the Sql statement to execute.
        /// </summary>
        IExecutableBuilder Sql(string sqlStatement);

        /// <summary>
        /// Specifies the Sql statement to execute.
        /// </summary>
        IExecutableBuilder Sql(Sql<ISqlContext> sql);
    }
}
