using System.Collections;
using NPoco;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

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

    public Sql<ISqlContext> Sql() => new Sql<ISqlContext>(_sqlContext, _sql);

    // must pass the args, all of them, in the proper order, faster
    public Sql<ISqlContext> Sql(params object[] args)
    {
        // if the type is an "unspeakable name" it is an anonymous compiler-generated object
        // see https://stackoverflow.com/questions/9256594
        // => assume it's an anonymous type object containing named arguments
        // (of course this means we cannot use *real* objects here and need SqlNamed - bah)
        if (args.Length == 1 && args[0].GetType().Name.Contains("<"))
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

        var isBuilt = !args.Any(x => x is IEnumerable);
        return new Sql<ISqlContext>(_sqlContext, isBuilt, _sql, args);
    }

    // can pass named args, not necessary all of them, slower
    // so, not much different from what Where(...) does (ie reflection)
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
    ///     Gets a WHERE expression argument.
    /// </summary>
    public static T? Arg<T>(string name) => default;

    /// <summary>
    ///     Gets a WHERE IN expression argument.
    /// </summary>
    public static IEnumerable<T?> ArgIn<T>(string name) =>

        // don't return an empty enumerable, as it breaks NPoco
        new[] { default(T) };

    // these are created in PocoToSqlExpressionVisitor
    internal class TemplateArg
    {
        public TemplateArg(string? name) => Name = name;

        public string? Name { get; }

        public override string ToString() => "@" + Name;
    }
}
