using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

public class RetryDbConnection : DbConnection
{
    private readonly RetryPolicy? _cmdRetryPolicy;
    private readonly RetryPolicy _conRetryPolicy;

    public RetryDbConnection(DbConnection connection, RetryPolicy? conRetryPolicy, RetryPolicy? cmdRetryPolicy)
    {
        Inner = connection;
        Inner.StateChange += StateChangeHandler;

        _conRetryPolicy = conRetryPolicy ?? RetryPolicy.NoRetry;
        _cmdRetryPolicy = cmdRetryPolicy;
    }

    public DbConnection Inner { get; }

    [AllowNull]
    public override string ConnectionString
    {
        get => Inner.ConnectionString;
        set => Inner.ConnectionString = value;
    }

    public override int ConnectionTimeout => Inner.ConnectionTimeout;

    protected override bool CanRaiseEvents => true;

    public override string DataSource => Inner.DataSource;

    public override string Database => Inner.Database;

    public override string ServerVersion => Inner.ServerVersion;

    public override ConnectionState State => Inner.State;

    public override void ChangeDatabase(string databaseName) => Inner.ChangeDatabase(databaseName);

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        Inner.BeginTransaction(isolationLevel);

    public override void Close() => Inner.Close();

    public override void EnlistTransaction(Transaction? transaction) => Inner.EnlistTransaction(transaction);

    protected override DbCommand CreateDbCommand() =>
        new FaultHandlingDbCommand(this, Inner.CreateCommand(), _cmdRetryPolicy);

    protected override void Dispose(bool disposing)
    {
        if (disposing && Inner != null)
        {
            Inner.StateChange -= StateChangeHandler;
            Inner.Dispose();
        }

        base.Dispose(disposing);
    }

    public override DataTable GetSchema() => Inner.GetSchema();

    public override DataTable GetSchema(string collectionName) => Inner.GetSchema(collectionName);

    public override DataTable GetSchema(string collectionName, string?[] restrictionValues) =>
        Inner.GetSchema(collectionName, restrictionValues);

    public override void Open() => _conRetryPolicy.ExecuteAction(Inner.Open);

    public void Ensure()
    {
        // verify whether or not the connection is valid and is open. This code may be retried therefore
        // it is important to ensure that a connection is re-established should it have previously failed
        if (State != ConnectionState.Open)
        {
            Open();
        }
    }

    private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArguments) =>
        OnStateChange(stateChangeEventArguments);
}

internal class FaultHandlingDbCommand : DbCommand
{
    private readonly RetryPolicy _cmdRetryPolicy;
    private RetryDbConnection _connection;

    public FaultHandlingDbCommand(RetryDbConnection connection, DbCommand command, RetryPolicy? cmdRetryPolicy)
    {
        _connection = connection;
        Inner = command;
        _cmdRetryPolicy = cmdRetryPolicy ?? RetryPolicy.NoRetry;
    }

    public DbCommand Inner { get; private set; }

    [AllowNull]
    public override string CommandText
    {
        get => Inner.CommandText;
        set => Inner.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => Inner.CommandTimeout;
        set => Inner.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => Inner.CommandType;
        set => Inner.CommandType = value;
    }

    public override bool DesignTimeVisible { get; set; }

    [AllowNull]
    protected override DbConnection DbConnection
    {
        get => _connection;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!(value is RetryDbConnection connection))
            {
                throw new ArgumentException("Value is not a FaultHandlingDbConnection instance.");
            }

            if (_connection != null && _connection != connection)
            {
                throw new Exception("Value is another FaultHandlingDbConnection instance.");
            }

            _connection = connection;
            Inner.Connection = connection.Inner;
        }
    }

    protected override DbParameterCollection DbParameterCollection => Inner.Parameters;

    protected override DbTransaction? DbTransaction
    {
        get => Inner.Transaction;
        set => Inner.Transaction = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => Inner.UpdatedRowSource;
        set => Inner.UpdatedRowSource = value;
    }

    public override void Cancel() => Inner.Cancel();

    public override int ExecuteNonQuery() => Execute(() => Inner.ExecuteNonQuery());

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Inner.Dispose();
        }

        Inner = null!;
        base.Dispose(disposing);
    }

    protected override DbParameter CreateDbParameter() => Inner.CreateParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
        Execute(() => Inner.ExecuteReader(behavior));

    public override object? ExecuteScalar() => Execute(() => Inner.ExecuteScalar());

    public override void Prepare() => Inner.Prepare();

    private T Execute<T>(Func<T> f) =>
        _cmdRetryPolicy.ExecuteAction(() =>
        {
            _connection.Ensure();
            return f();
        })!;
}
