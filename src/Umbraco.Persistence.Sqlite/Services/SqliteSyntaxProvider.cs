using System.Data;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="ISqlSyntaxProvider"/> for SQLite.
/// </summary>
public class SqliteSyntaxProvider : SqlSyntaxProviderBase<SqliteSyntaxProvider>
{
    /// <inheritdoc />
    public override string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public override IsolationLevel DefaultIsolationLevel => IsolationLevel.RepeatableRead;

    /// <inheritdoc />
    public override string DbProvider => Constants.ProviderName;

    /// <inheritdoc />
    public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db) => throw new NotImplementedException();

    /// <inheritdoc />
    public override bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName, out string constraintName) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void ReadLock(IDatabase db, TimeSpan timeout, int lockId) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void WriteLock(IDatabase db, TimeSpan timeout, int lockId) => throw new NotImplementedException();

    /// <inheritdoc />
    protected override string FormatSystemMethods(SystemMethods systemMethod) => throw new NotImplementedException();

    /// <inheritdoc />
    protected override string FormatIdentity(ColumnDefinition column) => throw new NotImplementedException();

    /// <inheritdoc />
    public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void ReadLock(IDatabase db, params int[] lockIds) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void WriteLock(IDatabase db, params int[] lockIds) => throw new NotImplementedException();
}
