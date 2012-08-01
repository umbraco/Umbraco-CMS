/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System.Text;
using System;

namespace umbraco.DataLayer
{
    /// <summary>
    /// Object that performs parsing tasks on an SQL command.
    /// </summary>
    public class SqlParser
    {
        /// <summary>Original tokens for query token replacement.</summary>
        private char[] m_SrcTokens;
        /// <summary>Destination tokens for query token replacement.</summary>
        private char[] m_DestTokens;

        #region Public Properties

        /// <summary>
        /// Gets the character strings are surrounded with.
        /// </summary>
        /// <value>The string delimiter.</value>
        public virtual char StringDelimiter
        {
            get { return '\''; }
        }

        /// <summary>
        /// Gets the param character parameters start with.
        /// </summary>
        /// <value>The parameter starting token.</value>
        public virtual char ParamToken
        {
            get { return '@'; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlParser"/> class.
        /// </summary>
        public SqlParser()
            : this(null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlParser"/> class
        /// that performs the specified token translation.
        /// </summary>
        /// <param name="srcTokens">The original tokens.</param>
        /// <param name="destTokens">The new tokens, each token corresponds with the item of
        /// <c>srcTokens</c> at the same index.</param>
        public SqlParser(char[] srcTokens, char[] destTokens)
        {
            if ((srcTokens == null && destTokens != null)
                || (srcTokens != null && destTokens == null)
                || (srcTokens != null && destTokens != null && srcTokens.Length != destTokens.Length))
                throw new ArgumentException("srcTokens and destTokens dimensions do not match");

            m_SrcTokens = srcTokens;
            m_DestTokens = destTokens;
        }

        #region Public Methods

        /// <summary>
        /// Parses the query, performing provider specific changes.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The modified query.</returns>
        /// <remarks>
        /// The default implementation returns the original query.
        /// Overriding classes can change this behavior.
        /// </remarks>
        public virtual string Parse(string query)
        {
            return query;
        }

        /// <summary>
        /// Replaces parameters in a query by their values.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The query, with parameter placeholders replaced by their values.</returns>
        public virtual string ApplyParameters(string commandText, IParameter[] parameters)
        {
            // no parameters, just return commandText
            if (parameters.Length == 0)
                return commandText;
            // build string that includes parameters
            StringBuilder result = new StringBuilder(commandText.Length);
            for (int i = 0; i < commandText.Length; i++)
            {
                // Is this the start of a parameter name?
                if (commandText[i] == ParamToken)
                {
                    /* Read parameter name, from ParamToken till next space or end */
                    int paramNameStart = i;
                    // move i past end of parameter name
                    while (++i < commandText.Length && char.IsLetterOrDigit(commandText[i])) ;
                    // store parameter name in string
                    string paramName = commandText.Substring(paramNameStart, i - paramNameStart).ToLower();
                    /* Get the parameter value */
                    string paramValue = string.Empty;
                    // find a parameter with identical name
                    for (int p = 0; p < parameters.Length && paramValue.Length == 0; p++)
                        if (parameters[p].ParameterName.ToLower() == paramName)
                            paramValue = parameters[p].Value.ToString();
                    /* Append parameter value */
                    result.Append('\'').Append(paramValue.Replace("'", "''")).Append('\'');
                    // check we're not at the end of the string
                    if (i >= commandText.Length)
                        break;
                }
                // append the character
                result.Append(commandText[i]);
            }
            // append terminating semicolon
            result.Append(';');
            return result.ToString();
        }

        /// <summary>Replaces tokens before or after identifiers in a query string.</summary>
        /// <param name="query">The original query.</param>
        /// <returns>The query with replaced identifier tokens.</returns>
        /// <remarks>Assumes a correct query.</remarks>
        public virtual string ReplaceIdentifierTokens(string query)
        {
            if (m_SrcTokens == null || m_DestTokens == null || m_SrcTokens.Length == 0 || m_DestTokens.Length == 0)
                return query;

            // find first occurrence of an originalToken
            bool noMatch = true;
            int pos;
            for (pos = 0; pos < query.Length && noMatch; pos++)
                foreach (char srcToken in m_SrcTokens)
                    if (query[pos] == srcToken)
                        noMatch = false;

            // if no occurrence found before end of string, return original query
            if (noMatch)
                return query;

            // put pos back at the match of the source token
            pos--;

            // if there's a quote before the token, it could be inside of a string
            for (int i = 0; i < pos; i++)
                if (query[i] == StringDelimiter)
                    // start at the beginning of the string
                    pos = i;
            // loop through all string characters
            char[] queryTokens = query.ToCharArray();
            do
            {
                char token = queryTokens[pos];
                // if begin of string, advance to end so we don't replace inside string literals
                if (token == StringDelimiter)
                    pos = FindStringEndPosition(query, pos);
                else
                {
                    // if we recognize a source token, replace it by a new one
                    for (int i = 0; i < m_SrcTokens.Length; i++)
                        if (token == m_SrcTokens[i])
                        {
                            queryTokens[pos] = m_DestTokens[i];
                            break;
                        }
                }
            } while (++pos < query.Length);
            // return new query
            return new string(queryTokens);
        }

        /// <summary>
        /// Uppercases the identifiers in the query, leaving strings untouched.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The query with uppercased identifiers.</returns>
        public virtual string UppercaseIdentifiers(string query)
        {
            if (string.IsNullOrEmpty(query))
                return string.Empty;

            StringBuilder replacedQuery = new StringBuilder(query.Length);

            // parse the query in SQL parts and string literal parts
            int partStartPos = 0;
            for (int currentPos = 0; currentPos < query.Length; currentPos++)
            {
                // String start?
                if (query[currentPos] == StringDelimiter)
                {
                    // append part before string, uppercased
                    replacedQuery.Append(query.Substring(partStartPos, currentPos - partStartPos).ToUpper());

                    // append string, case unchanged
                    int endStringPos = FindStringEndPosition(query, currentPos);
                    replacedQuery.Append(query.Substring(currentPos, endStringPos - currentPos));

                    // advance string
                    currentPos = partStartPos = endStringPos;
                }
            }
            // append remainder of the query, uppercased
            replacedQuery.Append(query.Substring(partStartPos).ToUpper());

            return replacedQuery.ToString();
        }

        #endregion

        #region Protected Methods

        /// <summary>Searches the position in a query where a string is terminated.</summary>
        /// <param name="query">The query to search.</param>
        /// <param name="startPos">The position of the opening quote of the string.</param>
        /// <returns>The position of termination quote of the string.</returns>
        /// <remarks>
        /// Assumes a correct query, 0&lt;startPos&lt;query.Length-1 and query[startPos]=='\''.
        /// (because the function is optimized for speed)
        /// </remarks>
        /// <exception cref="System.IndexOutOfRangeException">If the query is incorrect,
        /// startPos not in ]0,query.Length-1[ or query[startPos]!='\''. </exception>
        protected virtual int FindStringEndPosition(string query, int startPos)
        {
            // move to first character of string
            startPos++;
            // keep searching, function will eventually return with correct query
            // or encounter an IndexOutOfRangeException if incorrect
            while (true)
            {
                // move to next quote
                while (query[startPos++] != StringDelimiter) ;
                // if the quote is at the end of the string, we're done
                if (startPos == query.Length)
                    return startPos - 1;
                // if the next character isn't a quote, we're done also (quotes are escaped by doubling)
                if (query[startPos++] != StringDelimiter)
                    return startPos - 2;
            }
        }

        #endregion
    }
}