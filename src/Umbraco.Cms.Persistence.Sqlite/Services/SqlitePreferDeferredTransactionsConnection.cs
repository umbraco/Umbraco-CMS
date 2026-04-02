using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
/// A wrapper for SQLite connections that forces deferred transaction mode to prevent immediate write locks.
/// </summary>
public class SqlitePreferDeferredTransactionsConnection : DbConnection
{
    private readonly SqliteConnection _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlitePreferDeferredTransactionsConnection"/> class.
    /// </summary>
    /// <param name="inner">The inner SQLite connection to wrap.</param>
    public SqlitePreferDeferredTransactionsConnection(SqliteConnection inner) => _inner = inner;

    /// <inheritdoc />
    public override string Database
        => _inner.Database;

    /// <inheritdoc />
    public override ConnectionState State
        => _inner.State;

    /// <inheritdoc />
    public override string DataSource
        => _inner.DataSource;

    /// <inheritdoc />
    public override string ServerVersion
        => _inner.ServerVersion;

    /// <inheritdoc />
    [AllowNull]
    public override string ConnectionString
    {
        get => _inner.ConnectionString;
        set => _inner.ConnectionString = value;
    }

    /// <inheritdoc />
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        => _inner.BeginTransaction(isolationLevel, true); // <-- The important bit

    /// <inheritdoc />
    public override void ChangeDatabase(string databaseName)
        => _inner.ChangeDatabase(databaseName);

    /// <inheritdoc />
    public override void Close()
        => _inner.Close();

    /// <inheritdoc />
    public override void Open()
        => _inner.Open();

    /// <inheritdoc />
    protected override DbCommand CreateDbCommand()
        => new CommandWrapper(_inner.CreateCommand());

    private sealed class CommandWrapper : DbCommand
    {
        private readonly DbCommand _inner;

        public CommandWrapper(DbCommand inner) => _inner = inner;

        [AllowNull]
        public override string CommandText
        {
            get => _inner.CommandText;
            set => _inner.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _inner.CommandTimeout;
            set => _inner.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _inner.CommandType;
            set => _inner.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _inner.UpdatedRowSource;
            set => _inner.UpdatedRowSource = value;
        }

        protected override DbConnection? DbConnection
        {
            get => _inner.Connection;
            set => _inner.Connection = (value as SqlitePreferDeferredTransactionsConnection)?._inner;
        }

        protected override DbParameterCollection DbParameterCollection
            => _inner.Parameters;

        protected override DbTransaction? DbTransaction
        {
            get => _inner.Transaction;
            set => _inner.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _inner.DesignTimeVisible;
            set => _inner.DesignTimeVisible = value;
        }

        public override void Cancel()
            => _inner.Cancel();

        public override int ExecuteNonQuery()
            => _inner.ExecuteNonQuery();

        public override object? ExecuteScalar()
            => _inner.ExecuteScalar();

        public override void Prepare()
            => _inner.Prepare();

        protected override DbParameter CreateDbParameter()
            => _inner.CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => _inner.ExecuteReader(behavior);
    }
}
