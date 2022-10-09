using System.Linq.Expressions;
using System.Reflection;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Querying;

/// <summary>
///     Represents an expression tree parser used to turn strongly typed expressions into SQL statements.
/// </summary>
/// <typeparam name="TDto">The type of the DTO.</typeparam>
/// <remarks>This visitor is stateful and cannot be reused.</remarks>
internal class PocoToSqlExpressionVisitor<TDto> : ExpressionVisitorBase
{
    private readonly string? _alias;
    private readonly PocoData _pd;

    public PocoToSqlExpressionVisitor(ISqlContext sqlContext, string? alias)
        : base(sqlContext.SqlSyntax)
    {
        _pd = sqlContext.PocoDataFactory.ForType(typeof(TDto));
        _alias = alias;
    }

    protected override string VisitMethodCall(MethodCallExpression? m)
    {
        if (m is null)
        {
            return string.Empty;
        }

        Type? declaring = m.Method.DeclaringType;
        if (declaring != typeof(SqlTemplate))
        {
            return base.VisitMethodCall(m);
        }

        if (m.Method.Name != "Arg" && m.Method.Name != "ArgIn")
        {
            throw new NotSupportedException($"Method SqlTemplate.{m.Method.Name} is not supported.");
        }

        ParameterInfo[] parameters = m.Method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
        {
            throw new NotSupportedException(
                $"Method SqlTemplate.{m.Method.Name}({string.Join(", ", parameters.Select(x => x.ParameterType))} is not supported.");
        }

        Expression arg = m.Arguments[0];
        string? name;
        if (arg.NodeType == ExpressionType.Constant)
        {
            name = arg.ToString();
        }
        else
        {
            // though... we probably should avoid doing this
            UnaryExpression member = Expression.Convert(arg, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            Func<object> getter = lambda.Compile();
            name = getter().ToString();
        }

        SqlParameters.Add(new SqlTemplate.TemplateArg(RemoveQuote(name)));

        return Visited
            ? string.Empty
            : $"@{SqlParameters.Count - 1}";
    }

    protected override string VisitMemberAccess(MemberExpression? m)
    {
        if (m is null)
        {
            return string.Empty;
        }

        if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter &&
            m.Expression.Type == typeof(TDto))
        {
            return Visited ? string.Empty : GetFieldName(_pd, m.Member.Name, _alias);
        }

        if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
        {
            return Visited ? string.Empty : GetFieldName(_pd, m.Member.Name, _alias);
        }

        UnaryExpression member = Expression.Convert(m, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        Func<object> getter = lambda.Compile();
        var o = getter();

        SqlParameters.Add(o);

        return Visited ? string.Empty : "@" + (SqlParameters.Count - 1);
    }

    protected virtual string GetFieldName(PocoData pocoData, string name, string? alias)
    {
        KeyValuePair<string, PocoColumn> column =
            pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfoData.Name == name);
        var tableName = SqlSyntax.GetQuotedTableName(alias ?? pocoData.TableInfo.TableName);
        var columnName = SqlSyntax.GetQuotedColumnName(column.Value.ColumnName);

        return tableName + "." + columnName;
    }
}

/// <summary>
///     Represents an expression tree parser used to turn strongly typed expressions into SQL statements.
/// </summary>
/// <typeparam name="TDto1">The type of DTO 1.</typeparam>
/// <typeparam name="TDto2">The type of DTO 2.</typeparam>
/// <remarks>This visitor is stateful and cannot be reused.</remarks>
internal class PocoToSqlExpressionVisitor<TDto1, TDto2> : ExpressionVisitorBase
{
    private readonly string? _alias1;
    private readonly string? _alias2;
    private readonly PocoData _pocoData1;
    private readonly PocoData _pocoData2;
    private string? _parameterName1;
    private string? _parameterName2;

    public PocoToSqlExpressionVisitor(ISqlContext sqlContext, string? alias1, string? alias2)
        : base(sqlContext.SqlSyntax)
    {
        _pocoData1 = sqlContext.PocoDataFactory.ForType(typeof(TDto1));
        _pocoData2 = sqlContext.PocoDataFactory.ForType(typeof(TDto2));
        _alias1 = alias1;
        _alias2 = alias2;
    }

    protected override string VisitLambda(LambdaExpression? lambda)
    {
        if (lambda is null)
        {
            return string.Empty;
        }

        if (lambda.Parameters.Count == 2)
        {
            _parameterName1 = lambda.Parameters[0].Name;
            _parameterName2 = lambda.Parameters[1].Name;
        }
        else
        {
            _parameterName1 = _parameterName2 = null;
        }

        return base.VisitLambda(lambda);
    }

    protected override string VisitMemberAccess(MemberExpression? m)
    {
        if (m is null)
        {
            return string.Empty;
        }

        if (m.Expression != null)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                var pex = (ParameterExpression)m.Expression;

                if (pex.Name == _parameterName1)
                {
                    return Visited ? string.Empty : GetFieldName(_pocoData1, m.Member.Name, _alias1);
                }

                if (pex.Name == _parameterName2)
                {
                    return Visited ? string.Empty : GetFieldName(_pocoData2, m.Member.Name, _alias2);
                }
            }
            else if (m.Expression.NodeType == ExpressionType.Convert)
            {
                // here: which _pd should we use?!
                throw new NotSupportedException();

                // return Visited ? string.Empty : GetFieldName(_pd, m.Member.Name);
            }
        }

        UnaryExpression member = Expression.Convert(m, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        Func<object> getter = lambda.Compile();
        var o = getter();

        SqlParameters.Add(o);

        // execute if not already compiled
        return Visited ? string.Empty : "@" + (SqlParameters.Count - 1);
    }

    protected virtual string GetFieldName(PocoData pocoData, string name, string? alias)
    {
        KeyValuePair<string, PocoColumn> column =
            pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfoData.Name == name);
        var tableName = SqlSyntax.GetQuotedTableName(alias ?? pocoData.TableInfo.TableName);
        var columnName = SqlSyntax.GetQuotedColumnName(column.Value.ColumnName);

        return tableName + "." + columnName;
    }
}

/// <summary>
///     Represents an expression tree parser used to turn strongly typed expressions into SQL statements.
/// </summary>
/// <typeparam name="TDto1">The type of DTO 1.</typeparam>
/// <typeparam name="TDto2">The type of DTO 2.</typeparam>
/// <typeparam name="TDto3">The type of DTO 3.</typeparam>
/// <remarks>This visitor is stateful and cannot be reused.</remarks>
internal class PocoToSqlExpressionVisitor<TDto1, TDto2, TDto3> : ExpressionVisitorBase
{
    private readonly string? _alias1;
    private readonly string? _alias2;
    private readonly string? _alias3;
    private readonly PocoData _pocoData1;
    private readonly PocoData _pocoData2;
    private readonly PocoData _pocoData3;
    private string? _parameterName1;
    private string? _parameterName2;
    private string? _parameterName3;

    public PocoToSqlExpressionVisitor(ISqlContext sqlContext, string? alias1, string? alias2, string? alias3)
        : base(sqlContext.SqlSyntax)
    {
        _pocoData1 = sqlContext.PocoDataFactory.ForType(typeof(TDto1));
        _pocoData2 = sqlContext.PocoDataFactory.ForType(typeof(TDto2));
        _pocoData3 = sqlContext.PocoDataFactory.ForType(typeof(TDto3));
        _alias1 = alias1;
        _alias2 = alias2;
        _alias3 = alias3;
    }

    protected override string VisitLambda(LambdaExpression? lambda)
    {
        if (lambda is null)
        {
            return string.Empty;
        }

        if (lambda.Parameters.Count == 3)
        {
            _parameterName1 = lambda.Parameters[0].Name;
            _parameterName2 = lambda.Parameters[1].Name;
            _parameterName3 = lambda.Parameters[2].Name;
        }
        else if (lambda.Parameters.Count == 2)
        {
            _parameterName1 = lambda.Parameters[0].Name;
            _parameterName2 = lambda.Parameters[1].Name;
        }
        else
        {
            _parameterName1 = _parameterName2 = null;
        }

        return base.VisitLambda(lambda);
    }

    protected override string VisitMemberAccess(MemberExpression? m)
    {
        if (m is null)
        {
            return string.Empty;
        }

        if (m.Expression != null)
        {
            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                var pex = (ParameterExpression)m.Expression;

                if (pex.Name == _parameterName1)
                {
                    return Visited ? string.Empty : GetFieldName(_pocoData1, m.Member.Name, _alias1);
                }

                if (pex.Name == _parameterName2)
                {
                    return Visited ? string.Empty : GetFieldName(_pocoData2, m.Member.Name, _alias2);
                }

                if (pex.Name == _parameterName3)
                {
                    return Visited ? string.Empty : GetFieldName(_pocoData3, m.Member.Name, _alias3);
                }
            }
            else if (m.Expression.NodeType == ExpressionType.Convert)
            {
                // here: which _pd should we use?!
                throw new NotSupportedException();

                // return Visited ? string.Empty : GetFieldName(_pd, m.Member.Name);
            }
        }

        UnaryExpression member = Expression.Convert(m, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        Func<object> getter = lambda.Compile();
        var o = getter();

        SqlParameters.Add(o);

        // execute if not already compiled
        return Visited ? string.Empty : "@" + (SqlParameters.Count - 1);
    }

    protected virtual string GetFieldName(PocoData pocoData, string name, string? alias)
    {
        KeyValuePair<string, PocoColumn> column =
            pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfoData.Name == name);
        var tableName = SqlSyntax.GetQuotedTableName(alias ?? pocoData.TableInfo.TableName);
        var columnName = SqlSyntax.GetQuotedColumnName(column.Value.ColumnName);

        return tableName + "." + columnName;
    }
}
