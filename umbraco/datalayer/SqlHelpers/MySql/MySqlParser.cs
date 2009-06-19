/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.DataLayer.SqlHelpers.MySql
{
    /// <summary>
    /// Object that performs parsing tasks on a MySQL command.
    /// </summary>
    public class MySqlParser : SqlParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlParser"/> class.
        /// </summary>
        public MySqlParser()
            : base(new char[] { '[', ']', '@' }, new char[] { '`', '`', '?' })
        { }

        /// <summary>
        /// Parses the query, performing provider specific changes.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The modified query.</returns>
        /// <remarks>
        /// Uppercases identifiers and replaces their tokens.
        /// </remarks>
        public override string Parse(string query)
        {
            return UppercaseIdentifiers(ReplaceIdentifierTokens(query));
        }
    }
}