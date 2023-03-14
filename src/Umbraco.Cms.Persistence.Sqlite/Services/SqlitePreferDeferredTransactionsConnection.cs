using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

public class SqlitePreferDeferredTransactionsConnection : DbConnection
{
    private readonly SqliteConnection _inner;

    public SqlitePreferDeferredTransactionsConnection(SqliteConnection inner) => _inner = inner;

    public override string Database
        => _inner.Database;

    public override ConnectionState State
        => _inner.State;

    public override string DataSource
        => _inner.DataSource;

    public override string ServerVersion
        => _inner.ServerVersion;

    [AllowNull]
    public override string ConnectionString
    {
        get => _inner.ConnectionString;
        set => _inner.ConnectionString = value;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        => _inner.BeginTransaction(isolationLevel, true); // <-- The important bit

    public override void ChangeDatabase(string databaseName)
        => _inner.ChangeDatabase(databaseName);

    public override void Close()
        => _inner.Close();

    public override void Open()
        => _inner.Open();

    protected override DbCommand CreateDbCommand()
        => new CommandWrapper(_inner.CreateCommand());

    private class CommandWrapper : DbCommand
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
