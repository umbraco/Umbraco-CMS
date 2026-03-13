using System.Collections;
using NPoco;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Represents a template for generating SQL queries in the persistence layer.
/// </summary>
public class SqlTemplate
{
    private readonly Dictionary<int, object>? _args;
    private readonly string _sql;
    private readonly ISqlContext _sqlContext;

    internal SqlTemplate(ISqlContext sqlContext, string sql, object[] args)
    {
        _sqlContext = sqlContext;
        _sql = sql;
        if (args.Length > 0)
        {
            _args = new Dictionary<int, object>();
        }

        for (var i = 0; i < args.Length; i++)
        {
            _args![i] = args[i];
        }
    }

    /// <summary>
    ///     Gets a named argument.
    /// </summary>
    public static object Arg(string name) => new TemplateArg(name);

    /// <summary>
    /// Returns a new <see cref="Sql{ISqlContext}"/> instance initialized with the current SQL context and SQL string.
    /// </summary>
    /// <returns>A new <see cref="Sql{ISqlContext}"/> instance.</returns>
    public Sql<ISqlContext> Sql() => new Sql<ISqlContext>(_sqlContext, _sql);

    /// <summary>
    /// Creates a new SQL query from the template, using the specified arguments.
    /// </summary>
    /// <remarks>must pass the args, all of them, in the proper order, faster</remarks>
    /// <param name="args">Arguments to be formatted into the SQL template.</param>
    /// <returns>A new <see cref="Sql&lt;ISqlContext&gt;"/> instance representing the formatted SQL query.</returns>
    ///
    public Sql<ISqlContext> Sql(params object[] args)
    {
        // if the type is an "unspeakable name" it is an anonymous compiler-generated object
        // see https://stackoverflow.com/questions/9256594
        // => assume it's an anonymous type object containing named arguments
        // (of course this means we cannot use *real* objects here and need SqlNamed - bah)
        if (args.Length == 1 && args[0].GetType().Name.Contains('<'))
        {
            return SqlNamed(args[0]);
        }

        if (args.Length != _args?.Count)
        {
            throw new ArgumentException("Invalid number of arguments.", nameof(args));
        }

        if (args.Length == 0)
        {
            return new Sql<ISqlContext>(_sqlContext, true, _sql);
        }

        var isBuilt = !Array.Exists(args, x => x is IEnumerable);
        return new Sql<ISqlContext>(_sqlContext, isBuilt, _sql, args);
    }

    /// <summary>
    /// Creates a SQL query using named arguments provided via an object's properties.
    /// </summary>
    /// <param name="nargs">An object whose public properties are used as named arguments for the SQL query template.</param>
    /// <returns>A <see cref="Sql&lt;ISqlContext&gt;" /> instance representing the constructed SQL query.</returns>
    /// <remarks>
    /// can pass named args, not necessary all of them, slower
    /// so, not much different from what Where(...) does (ie reflection)
    /// Throws <see cref="InvalidOperationException"/> if required arguments are missing or if unknown arguments are provided.
    /// This method uses reflection to match property names to template argument names.
    /// </remarks>
    public Sql<ISqlContext> SqlNamed(object nargs)
    {
        var isBuilt = true;
        var args = new object[_args?.Count ?? 0];
        var properties = nargs.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(nargs));
        for (var i = 0; i < _args?.Count; i++)
        {
            object? value;
            if (_args[i] is TemplateArg templateArg)
            {
                if (!properties.TryGetValue(templateArg.Name ?? string.Empty, out value))
                {
                    throw new InvalidOperationException($"Missing argument \"{templateArg.Name}\".");
                }

                properties.Remove(templateArg.Name!);
            }
            else
            {
                value = _args[i];
            }

            args[i] = value!;

            // if value is enumerable then we'll need to expand arguments
            if (value is IEnumerable)
            {
                isBuilt = false;
            }
        }

        if (properties.Count > 0)
        {
            throw new InvalidOperationException(
                $"Unknown argument{(properties.Count > 1 ? "s" : string.Empty)}: {string.Join(", ", properties.Keys)}");
        }

        return new Sql<ISqlContext>(_sqlContext, isBuilt, _sql, args);
    }

    internal string ToText()
    {
        var sql = new Sql<ISqlContext>(_sqlContext, _sql, _args?.Values.ToArray());
        return sql.ToText();
    }

    /// <summary>
    ///     Retrieves the value of a named argument used in a WHERE expression.
    /// </summary>
    /// <param name="name">The name of the argument to retrieve.</param>
    /// <typeparam name="T">The expected type of the argument value.</typeparam>
    /// <returns>The argument value of type <typeparamref name="T"/> if found; otherwise, <c>null</c>.</returns>
    public static T? Arg<T>(string name) => default;

    /// <summary>
    ///     Returns an enumerable representing the argument for a SQL WHERE IN clause.
    /// </summary>
    /// <param name="name">The name of the argument to be used in the SQL template.</param>
    /// <typeparam name="T">The type of the elements in the argument list.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> containing a single default value of type <typeparamref name="T"/>. This is used to ensure compatibility with NPoco when no values are provided.</returns>
    public static IEnumerable<T?> ArgIn<T>(string name) =>

        // don't return an empty enumerable, as it breaks NPoco
        new[] { default(T) };

    // these are created in PocoToSqlExpressionVisitor
    internal sealed class TemplateArg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlTemplate.TemplateArg"/> class.
        /// </summary>
        /// <param name="name">The name of the template argument, or <c>null</c> if not specified.</param>
        public TemplateArg(string? name) => Name = name;

        /// <summary>
        /// Gets the name of the template argument.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Returns a string that represents the template argument, consisting of the argument name prefixed with '@'.
        /// </summary>
        /// <returns>A string representation of the template argument, prefixed with '@'.</returns>
        public override string ToString() => "@" + Name;
    }
}
