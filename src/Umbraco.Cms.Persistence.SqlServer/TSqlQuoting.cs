namespace Umbraco.Cms.Persistence.SqlServer;

/// <summary>
///     Quoting helpers for composing T-SQL statements that cannot be parameterised, such as
///     <c>CREATE DATABASE</c>.
/// </summary>
internal static class TSqlQuoting
{
    /// <summary>
    ///     Returns a Unicode string with the delimiters added to make the input string a valid SQL Server delimited
    ///     identifier.
    /// </summary>
    /// <param name="name">The name to quote.</param>
    /// <param name="quote">A quote character.</param>
    /// <returns>The quoted name, with any embedded delimiter doubled.</returns>
    /// <remarks>
    ///     This is a C# implementation of T-SQL QUOTENAME.
    ///     <paramref name="quote" /> is optional, it can be '[' (default), ']', '\'' or '"'.
    /// </remarks>
    internal static string QuotedName(string name, char quote = '[')
    {
        switch (quote)
        {
            case '[':
            case ']':
                return "[" + name.Replace("]", "]]") + "]";
            case '\'':
                return "'" + name.Replace("'", "''") + "'";
            case '"':
                return "\"" + name.Replace("\"", "\"\"") + "\"";
            default:
                throw new NotSupportedException("Not a valid quote character.");
        }
    }
}
