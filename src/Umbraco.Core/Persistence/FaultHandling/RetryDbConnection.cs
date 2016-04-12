using System;
using System.Data;
using System.Data.Common;
using NPoco;
using Transaction = System.Transactions.Transaction;

namespace Umbraco.Core.Persistence.FaultHandling
{
    class RetryDbConnection : DbConnection
    {
        private DbConnection _inner;
        private readonly RetryPolicy _conRetryPolicy;
        private readonly RetryPolicy _cmdRetryPolicy;

        public RetryDbConnection(DbConnection connection, RetryPolicy conRetryPolicy, RetryPolicy cmdRetryPolicy)
        {
            _inner = connection;
            _inner.StateChange += StateChangeHandler;

            _conRetryPolicy = conRetryPolicy ?? RetryPolicy.NoRetry;
            _cmdRetryPolicy = cmdRetryPolicy;
        }

        public DbConnection Inner { get { return _inner; } }

        public override string ConnectionString { get { return _inner.ConnectionString; } set { _inner.ConnectionString = value; } }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _inner.BeginTransaction(isolationLevel);
        }

        protected override bool CanRaiseEvents
        {
            get { return true; }
        }

        public override void ChangeDatabase(string databaseName)
        {
            _inner.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _inner.Close();
        }

        public override int ConnectionTimeout
        {
            get { return _inner.ConnectionTimeout; }
        }

        protected override DbCommand CreateDbCommand()
        {
            return new FaultHandlingDbCommand(this, _inner.CreateCommand(), _cmdRetryPolicy);
        }

        public override string DataSource
        {
            get { return _inner.DataSource; }
        }

        public override string Database
        {
            get { return _inner.Database; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _inner != null)
            {
                _inner.StateChange -= StateChangeHandler;
                _inner.Dispose();
            }
            _inner = null;
            base.Dispose(disposing);
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            _inner.EnlistTransaction(transaction);
        }

        public override DataTable GetSchema()
        {
            return _inner.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return _inner.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _inner.GetSchema(collectionName, restrictionValues);
        }

        public override void Open()
        {
            _conRetryPolicy.ExecuteAction(_inner.Open);
        }

        public override string ServerVersion
        {
            get { return _inner.ServerVersion; }
        }

        public override ConnectionState State
        {
            get { return _inner.State; }
        }

        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArguments)
        {
            OnStateChange(stateChangeEventArguments);
        }

        public void Ensure()
        {
            // verify whether or not the connection is valid and is open. This code may be retried therefore
            // it is important to ensure that a connection is re-established should it have previously failed
            if (State != ConnectionState.Open)
                Open();
        }
    }

    class FaultHandlingDbCommand : DbCommand
    {
        private RetryDbConnection _connection;
        private DbCommand _inner;
        private readonly RetryPolicy _cmdRetryPolicy;

        public FaultHandlingDbCommand(RetryDbConnection connection, DbCommand command, RetryPolicy cmdRetryPolicy)
        {
            _connection = connection;
            _inner = command;
            _cmdRetryPolicy = cmdRetryPolicy ?? RetryPolicy.NoRetry;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _inner != null)
                _inner.Dispose();
            _inner = null;
            base.Dispose(disposing);
        }

        public override void Cancel()
        {
            _inner.Cancel();
        }

        public override string CommandText { get { return _inner.CommandText; } set { _inner.CommandText = value; } }

        public override int CommandTimeout { get { return _inner.CommandTimeout; } set { _inner.CommandTimeout = value; } }

        public override CommandType CommandType { get { return _inner.CommandType; } set { _inner.CommandType = value; } }

        protected override DbConnection DbConnection
        {
            get
            {
                return _connection;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                var connection = value as RetryDbConnection;
                if (connection == null) throw new ArgumentException("Value is not a FaultHandlingDbConnection instance.");
                if (_connection != null && _connection != connection) throw new Exception("Value is another FaultHandlingDbConnection instance.");
                _connection = connection;
                _inner.Connection = connection.Inner;
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return _inner.CreateParameter();
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _inner.Parameters; }
        }

        protected override DbTransaction DbTransaction { get { return _inner.Transaction; } set { _inner.Transaction = value; } }

        public override bool DesignTimeVisible { get; set; }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return Execute(() => _inner.ExecuteReader(behavior));
        }

        public override int ExecuteNonQuery()
        {
            return Execute(() => _inner.ExecuteNonQuery());
        }

        public override object ExecuteScalar()
        {
            return Execute(() => _inner.ExecuteScalar());
        }

        private T Execute<T>(Func<T> f)
        {
            return _cmdRetryPolicy.ExecuteAction(() =>
            {
                _connection.Ensure();
                return f();
            });
        }

        public override void Prepare()
        {
            _inner.Prepare();
        }

        public override UpdateRowSource UpdatedRowSource { get { return _inner.UpdatedRowSource; } set { _inner.UpdatedRowSource = value; } }
    }
}
