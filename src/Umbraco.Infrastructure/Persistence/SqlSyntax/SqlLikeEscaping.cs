namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

// TODO (V20): Remove, moving the implementation into SqlSyntaxProviderBase once the ISqlSyntaxProvider default implementations are removed.

/// <summary>
///     Escapes LIKE wildcard characters using a backslash as the escape character.
/// </summary>
internal static class SqlLikeEscaping
{
    /// <summary>
    ///     The <c>ESCAPE</c> clause matching <see cref="EscapeValue" />.
    /// </summary>
    public const string EscapeClause = @"ESCAPE '\'";

    /// <summary>
    ///     Escapes the backslash, <c>%</c> and <c>_</c> characters in a value.
    /// </summary>
    /// <param name="value">The text to match literally.</param>
    /// <returns>The escaped value.</returns>
    public static string EscapeValue(string value)
        => value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
