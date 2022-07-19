namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     Provides a mean to express aliases in SELECT Sql statements.
/// </summary>
/// <remarks>
///     <para>
///         First register with <c>using static Umbraco.Core.Persistence.NPocoSqlExtensions.Aliaser</c>,
///         then use eg <c>Sql{Foo}(x => Alias(x.Id, "id"))</c>.
///     </para>
/// </remarks>
public static class SqlExtensionsStatics
{
    /// <summary>
    ///     Aliases a field.
    /// </summary>
    /// <param name="field">The field to alias.</param>
    /// <param name="alias">The alias.</param>
    public static object? Alias(object? field, string alias) => field;

    /// <summary>
    ///     Produces Sql text.
    /// </summary>
    /// <param name="field">The name of the field.</param>
    /// <param name="expr">A function producing Sql text.</param>
    public static T? SqlText<T>(string field, Func<string, string> expr) => default;

    /// <summary>
    ///     Produces Sql text.
    /// </summary>
    /// <param name="field1">The name of the first field.</param>
    /// <param name="field2">The name of the second field.</param>
    /// <param name="expr">A function producing Sql text.</param>
    public static T? SqlText<T>(string field1, string field2, Func<string, string, string> expr) => default;

    /// <summary>
    ///     Produces Sql text.
    /// </summary>
    /// <param name="field1">The name of the first field.</param>
    /// <param name="field2">The name of the second field.</param>
    /// <param name="field3">The name of the third field.</param>
    /// <param name="expr">A function producing Sql text.</param>
    public static T? SqlText<T>(string field1, string field2, string field3, Func<string, string, string, string> expr) => default;
}
