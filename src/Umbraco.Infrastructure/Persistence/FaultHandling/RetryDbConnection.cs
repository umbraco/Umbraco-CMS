using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

/// <summary>
/// Provides a database connection implementation that automatically retries operations when transient failures occur, improving reliability.
/// </summary>
public class RetryDbConnection : DbConnection
{
    private readonly RetryPolicy? _cmdRetryPolicy;
    private readonly RetryPolicy _conRetryPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDbConnection"/> class with the specified connection and retry policies.
    /// </summary>
    /// <param name="connection">The database connection to wrap with retry logic.</param>
    /// <param name="conRetryPolicy">The retry policy to apply to the connection operations.</param>
    /// <param name="cmdRetryPolicy">The retry policy to apply to the command operations.</param>
    public RetryDbConnection(DbConnection connection, RetryPolicy? conRetryPolicy, RetryPolicy? cmdRetryPolicy)
    {
        Inner = connection;
        Inner.StateChange += StateChangeHandler;

        _conRetryPolicy = conRetryPolicy ?? RetryPolicy.NoRetry;
        _cmdRetryPolicy = cmdRetryPolicy;
    }

    /// <summary>
    /// Gets the inner <see cref="DbConnection" /> instance.
    /// </summary>
    public DbConnection Inner { get; }

    /// <summary>
    /// Gets or sets the connection string that is used by this database connection instance.
    /// </summary>
    [AllowNull]
    public override string ConnectionString
    {
        get => Inner.ConnectionString;
        set => Inner.ConnectionString = value;
    }

    /// <summary>
    /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
    /// </summary>
    public override int ConnectionTimeout => Inner.ConnectionTimeout;

    protected override bool CanRaiseEvents => true;

    /// <summary>
    /// Gets the data source of the inner database connection.
    /// </summary>
    public override string DataSource => Inner.DataSource;

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public override string Database => Inner.Database;

    /// <summary>
    /// Gets the version of the database server.
    /// </summary>
    public override string ServerVersion => Inner.ServerVersion;

    /// <summary>
    /// Gets the current state of the underlying database connection.
    /// </summary>
    public override ConnectionState State => Inner.State;

    /// <summary>
    /// Changes the current database for the connection by delegating the operation to the underlying connection.
    /// </summary>
    /// <param name="databaseName">The name of the database to switch to.</param>
    public override void ChangeDatabase(string databaseName) => Inner.ChangeDatabase(databaseName);

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        Inner.BeginTransaction(isolationLevel);

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    public override void Close() => Inner.Close();

    /// <summary>
    /// Enlists the specified transaction with the current connection.
    /// </summary>
    /// <param name="transaction">The transaction to enlist, or null to clear the enlistment.</param>
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

    /// <summary>
    /// Retrieves schema information from the underlying database connection.
    /// </summary>
    /// <returns>A <see cref="System.Data.DataTable"/> containing schema information about the database.</returns>
    public override DataTable GetSchema() => Inner.GetSchema();

    /// <summary>Gets schema information from the data source.</summary>
    /// <param name="collectionName">The name of the schema collection to retrieve.</param>
    /// <returns>A <see cref="DataTable"/> that contains schema information.</returns>
    public override DataTable GetSchema(string collectionName) => Inner.GetSchema(collectionName);

    /// <summary>
    /// Retrieves schema information from the underlying database connection for the specified collection name and restriction values.
    /// </summary>
    /// <param name="collectionName">The name of the schema collection to retrieve.</param>
    /// <param name="restrictionValues">An array of restriction values to filter the schema information, or <c>null</c> to apply no restrictions.</param>
    /// <returns>A <see cref="System.Data.DataTable"/> containing the requested schema information.</returns>
    public override DataTable GetSchema(string collectionName, string?[] restrictionValues) =>
        Inner.GetSchema(collectionName, restrictionValues);

    /// <summary>
    /// Opens the database connection, retrying on failure according to the configured retry policy.
    /// </summary>
    public override void Open() => _conRetryPolicy.ExecuteAction(Inner.Open);

    /// <summary>
    /// Ensures that the database connection is valid and open, reopening it if necessary.
    /// </summary>
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

internal sealed class FaultHandlingDbCommand : DbCommand
{
    private readonly RetryPolicy _cmdRetryPolicy;
    private RetryDbConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="FaultHandlingDbCommand"/> class.
    /// </summary>
    /// <param name="connection">The <see cref="RetryDbConnection"/> that provides retry logic for database operations.</param>
    /// <param name="command">The underlying <see cref="DbCommand"/> to be executed.</param>
    /// <param name="cmdRetryPolicy">The <see cref="RetryPolicy"/> to apply to this command, or <c>null</c> if no retry policy is specified.</param>
    public FaultHandlingDbCommand(RetryDbConnection connection, DbCommand command, RetryPolicy? cmdRetryPolicy)
    {
        _connection = connection;
        Inner = command;
        _cmdRetryPolicy = cmdRetryPolicy ?? RetryPolicy.NoRetry;
    }

    /// <summary>
    /// Gets the inner <see cref="DbCommand"/> instance.
    /// </summary>
    public DbCommand Inner { get; private set; }

    /// <summary>
    /// Gets or sets the text command to run against the data source.
    /// </summary>
    [AllowNull]
    public override string CommandText
    {
        get => Inner.CommandText;
        set => Inner.CommandText = value;
    }

    /// <summary>
    /// Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
    /// </summary>
    public override int CommandTimeout
    {
        get => Inner.CommandTimeout;
        set => Inner.CommandTimeout = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="CommandType"/> value for the underlying database command executed by this instance.
    /// </summary>
    public override CommandType CommandType
    {
        get => Inner.CommandType;
        set => Inner.CommandType = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the command is visible in a designer.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating how the results of a command are applied to a <see cref="DataRow"/> when used by the <see cref="DbDataAdapter.Update"/> method.
    /// This property determines whether output parameters and/or the first returned row are mapped to the changed row in the data table.
    /// </summary>
    public override UpdateRowSource UpdatedRowSource
    {
        get => Inner.UpdatedRowSource;
        set => Inner.UpdatedRowSource = value;
    }

    /// <summary>
    /// Cancels the execution of the current database command.
    /// This attempts to stop the command if it is currently running.
    /// </summary>
    public override void Cancel() => Inner.Cancel();

    /// <summary>
    /// Executes a SQL statement against the database connection, applying fault handling logic, and returns the number of rows affected.
    /// </summary>
    /// <returns>The number of rows affected by the SQL statement.</returns>
    /// <remarks>
    /// This method overrides the base implementation to provide retry or fault handling capabilities.
    /// </remarks>
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

    /// <summary>Executes the query and returns the first column of the first row in the result set returned by the query.</summary>
    /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
    public override object? ExecuteScalar() => Execute(() => Inner.ExecuteScalar());

    /// <summary>
    /// Prepares the underlying database command for execution by calling <see cref="DbCommand.Prepare"/> on the wrapped command.
    /// </summary>
    public override void Prepare() => Inner.Prepare();

    private T Execute<T>(Func<T> f) =>
        _cmdRetryPolicy.ExecuteAction(() =>
        {
            _connection.Ensure();
            return f();
        })!;
}
