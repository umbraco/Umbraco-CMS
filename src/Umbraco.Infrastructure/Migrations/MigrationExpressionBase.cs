using System.Text;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Provides a base class for migration expressions.
/// </summary>
public abstract class MigrationExpressionBase : IMigrationExpression
{
    private bool _executed;
    private List<IMigrationExpression>? _expressions;

    protected MigrationExpressionBase(IMigrationContext context) =>
        Context = context ?? throw new ArgumentNullException(nameof(context));

    public DatabaseType DatabaseType => Context.Database.DatabaseType;

    protected IMigrationContext Context { get; }

    protected ILogger Logger => Context.Logger;

    protected ISqlSyntaxProvider SqlSyntax => Context.Database.SqlContext.SqlSyntax;

    protected IUmbracoDatabase Database => Context.Database;

    public List<IMigrationExpression> Expressions => _expressions ??= new List<IMigrationExpression>();

    /// <summary>
    /// This might be useful in the future if we add it to the interface, but for now it's used to hack the DeleteAppTables & DeleteForeignKeyExpression
    /// to ensure they are not executed twice.
    /// </summary>
    internal string? Name { get; set; }

    public virtual void Execute()
    {
        if (_executed)
        {
            throw new InvalidOperationException("This expression has already been executed.");
        }

        _executed = true;
        Context.BuildingExpression = false;

        var sql = GetSql();

        if (string.IsNullOrWhiteSpace(sql))
        {
            Logger.LogInformation("SQL [{ContextIndex}]: <empty>", Context.Index);
        }
        else
        {
            // split multiple statements - required for SQL CE
            // http://stackoverflow.com/questions/13665491/sql-ce-inconsistent-with-multiple-statements
            var stmtBuilder = new StringBuilder();
            using (var reader = new StringReader(sql))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                    {
                        ExecuteStatement(stmtBuilder);
                    }
                    else
                    {
                        stmtBuilder.Append(line);
                    }
                }

                if (stmtBuilder.Length > 0)
                {
                    ExecuteStatement(stmtBuilder);
                }
            }
        }

        Context.Index++;

        if (_expressions == null)
        {
            return;
        }

        // HACK: We're handling all the constraints higher up the stack for SQLite.
            if (Context.Database.DatabaseType.IsSqlite())
            {
                _expressions = _expressions
                    .Where(x => x is not CreateConstraintExpression)
                    .Where(x => x is not CreateForeignKeyExpression)
                    .ToList();
            }

            foreach (IMigrationExpression expression in _expressions)
        {
            expression.Execute();
        }
    }

    protected virtual string? GetSql() => ToString();

    protected void Execute(Sql<ISqlContext>? sql)
    {
        if (_executed)
        {
            throw new InvalidOperationException("This expression has already been executed.");
        }

        _executed = true;
        Context.BuildingExpression = false;

        if (sql == null)
        {
            Logger.LogInformation($"SQL [{Context.Index}]: <empty>");
        }
        else
        {
            Logger.LogInformation($"SQL [{Context.Index}]: {sql.ToText()}");
            Database.Execute(sql);
        }

        Context.Index++;

        if (_expressions == null)
        {
            return;
        }

        foreach (IMigrationExpression expression in _expressions)
        {
            expression.Execute();
        }
    }

    protected void AppendStatementSeparator(StringBuilder stmtBuilder)
    {
        stmtBuilder.AppendLine(";");
        if (DatabaseType.IsSqlServer())
        {
            stmtBuilder.AppendLine("GO");
        }
    }

    private void ExecuteStatement(StringBuilder stmtBuilder)
    {
        var stmt = stmtBuilder.ToString();
        Logger.LogInformation("SQL [{ContextIndex}]: {Sql}", Context.Index, stmt);
        Database.Execute(stmt);
        stmtBuilder.Clear();
    }

    protected string GetQuotedValue(object? val)
    {
        if (val == null)
        {
            return "NULL";
        }

        Type type = val.GetType();

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return (bool)val ? "1" : "0";
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return val.ToString()!;
            case TypeCode.DateTime:
                return SqlSyntax.GetQuotedValue(SqlSyntax.FormatDateTime((DateTime)val));
            default:
                return SqlSyntax.GetQuotedValue(val.ToString()!);
        }
    }
}
