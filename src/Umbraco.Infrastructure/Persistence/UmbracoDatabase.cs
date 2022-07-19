using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Extends NPoco Database for Umbraco.
/// </summary>
/// <remarks>
///     <para>
///         Is used everywhere in place of the original NPoco Database object, and provides additional features
///         such as profiling, retry policies, logging, etc.
///     </para>
///     <para>Is never created directly but obtained from the <see cref="UmbracoDatabaseFactory" />.</para>
/// </remarks>
public class UmbracoDatabase : Database, IUmbracoDatabase
{
    private readonly ILogger<UmbracoDatabase> _logger;
    private readonly IBulkSqlInsertProvider? _bulkSqlInsertProvider;
    private readonly DatabaseSchemaCreatorFactory? _databaseSchemaCreatorFactory;
    private readonly IEnumerable<IMapper>? _mapperCollection;
    private readonly Guid _instanceGuid = Guid.NewGuid();
    private List<CommandInfo>? _commands;

    #region Ctor

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoDatabase" /> class.
    /// </summary>
    /// <remarks>
    ///     <para>Used by UmbracoDatabaseFactory to create databases.</para>
    ///     <para>Also used by DatabaseBuilder for creating databases and installing/upgrading.</para>
    /// </remarks>
    public UmbracoDatabase(
        string connectionString,
        ISqlContext sqlContext,
        DbProviderFactory provider,
        ILogger<UmbracoDatabase> logger,
        IBulkSqlInsertProvider? bulkSqlInsertProvider,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IEnumerable<IMapper>? mapperCollection = null)
        : base(connectionString, sqlContext.DatabaseType, provider, sqlContext.SqlSyntax.DefaultIsolationLevel)
    {
        SqlContext = sqlContext;
        _logger = logger;
        _bulkSqlInsertProvider = bulkSqlInsertProvider;
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        _mapperCollection = mapperCollection;

        Init();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoDatabase" /> class.
    /// </summary>
    /// <remarks>Internal for unit tests only.</remarks>
    internal UmbracoDatabase(
        DbConnection connection,
        ISqlContext sqlContext,
        ILogger<UmbracoDatabase> logger,
        IBulkSqlInsertProvider bulkSqlInsertProvider)
        : base(connection, sqlContext.DatabaseType, sqlContext.SqlSyntax.DefaultIsolationLevel)
    {
        SqlContext = sqlContext;
        _logger = logger;
        _bulkSqlInsertProvider = bulkSqlInsertProvider;

        Init();
    }

    private void Init()
    {
        EnableSqlTrace = EnableSqlTraceDefault;
        NPocoDatabaseExtensions.ConfigureNPocoBulkExtensions();

        if (_mapperCollection != null)
        {
            Mappers.AddRange(_mapperCollection);
        }
    }

    #endregion

    /// <inheritdoc />
    public ISqlContext SqlContext { get; }

    #region Testing, Debugging and Troubleshooting

    private bool _enableCount;

#if DEBUG_DATABASES
        private int _spid = -1;
        private const bool EnableSqlTraceDefault = true;
#else
    private string? _instanceId;
    private const bool EnableSqlTraceDefault = false;
#endif

    /// <inheritdoc />
    public string InstanceId =>
#if DEBUG_DATABASES
                _instanceGuid.ToString("N").Substring(0, 8) + ':' + _spid;
#else
        _instanceId ??= _instanceGuid.ToString("N").Substring(0, 8);
#endif

    /// <inheritdoc />
    public bool InTransaction { get; private set; }

    protected override void OnBeginTransaction()
    {
        base.OnBeginTransaction();
        InTransaction = true;
    }

    protected override void OnAbortTransaction()
    {
        InTransaction = false;
        base.OnAbortTransaction();
    }

    protected override void OnCompleteTransaction()
    {
        InTransaction = false;
        base.OnCompleteTransaction();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to log all executed Sql statements.
    /// </summary>
    internal bool EnableSqlTrace { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to count all executed Sql statements.
    /// </summary>
    public bool EnableSqlCount
    {
        get => _enableCount;
        set
        {
            _enableCount = value;

            if (_enableCount == false)
            {
                SqlCount = 0;
            }
        }
    }

    /// <summary>
    ///     Gets the count of all executed Sql statements.
    /// </summary>
    public int SqlCount { get; private set; }

    internal bool LogCommands
    {
        get => _commands != null;
        set => _commands = value ? new List<CommandInfo>() : null;
    }

    internal IEnumerable<CommandInfo>? Commands => _commands;

    public int BulkInsertRecords<T>(IEnumerable<T> records) =>
        _bulkSqlInsertProvider?.BulkInsertRecords(this, records) ?? 0;

    /// <summary>
    ///     Returns the <see cref="DatabaseSchemaResult" /> for the database
    /// </summary>
    public DatabaseSchemaResult ValidateSchema()
    {
        DatabaseSchemaCreator? dbSchema = _databaseSchemaCreatorFactory?.Create(this);
        DatabaseSchemaResult? databaseSchemaValidationResult = dbSchema?.ValidateSchema();

        return databaseSchemaValidationResult ?? new DatabaseSchemaResult();
    }

    /// <summary>
    ///     Returns true if Umbraco database tables are detected to be installed
    /// </summary>
    public bool IsUmbracoInstalled() => ValidateSchema().DetermineHasInstalledVersion();

    #endregion

    #region OnSomething

    protected override DbConnection OnConnectionOpened(DbConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        // TODO: this should probably move to a SQL Server ProviderSpecificInterceptor.
#if DEBUG_DATABASES
            // determines the database connection SPID for debugging
            if (DatabaseType.IsSqlServer())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @@SPID";
                    _spid = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            else
            {
                // includes SqlCE
                _spid = 0;
            }

#endif
        return connection;
    }

#if DEBUG_DATABASES
        protected override void OnConnectionClosing(DbConnection conn)
        {
            _spid = -1;
            base.OnConnectionClosing(conn);
        }
#endif

    protected override void OnException(Exception ex)
    {
        _logger.LogError(ex, "Exception ({InstanceId}).", InstanceId);
        _logger.LogDebug("At:\r\n{StackTrace}", Environment.StackTrace);

        if (EnableSqlTrace == false)
        {
            _logger.LogDebug("Sql:\r\n{Sql}", CommandToString(LastSQL, LastArgs));
        }

        base.OnException(ex);
    }

    private DbCommand? _cmd;

    protected override void OnExecutingCommand(DbCommand cmd)
    {
        // if no timeout is specified, and the connection has a longer timeout, use it
        if (OneTimeCommandTimeout == 0 && CommandTimeout == 0 && cmd.Connection?.ConnectionTimeout > 30)
        {
            cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
        }

        if (EnableSqlTrace)
        {
            _logger.LogDebug("SQL Trace:\r\n{Sql}", CommandToString(cmd).Replace("{", "{{").Replace("}", "}}")); // TODO: these escapes should be builtin
        }

#if DEBUG_DATABASES
            // detects whether the command is already in use (eg still has an open reader...)
            DatabaseDebugHelper.SetCommand(cmd, InstanceId + " [T" + System.Threading.Thread.CurrentThread.ManagedThreadId + "]");
            var refsobj = DatabaseDebugHelper.GetReferencedObjects(cmd.Connection);
            if (refsobj != null) _logger.LogDebug("Oops!" + Environment.NewLine + refsobj);
#endif

        _cmd = cmd;

        base.OnExecutingCommand(cmd);
    }

    private string CommandToString(DbCommand cmd) => CommandToString(cmd.CommandText, cmd.Parameters.Cast<DbParameter>().Select(x => x.Value).WhereNotNull().ToArray());

    private string CommandToString(string? sql, object[]? args)
    {
        var text = new StringBuilder();
#if DEBUG_DATABASES
            text.Append(InstanceId);
            text.Append(": ");
#endif

        NPocoSqlExtensions.ToText(sql, args, text);

        return text.ToString();
    }

    protected override void OnExecutedCommand(DbCommand cmd)
    {
        if (_enableCount)
        {
            SqlCount++;
        }

        _commands?.Add(new CommandInfo(cmd));

        base.OnExecutedCommand(cmd);
    }

    #endregion

    // used for tracking commands
    public class CommandInfo
    {
        public CommandInfo(IDbCommand cmd)
        {
            Text = cmd.CommandText;
            var parameters = new List<ParameterInfo>();
            foreach (IDbDataParameter parameter in cmd.Parameters)
            {
                parameters.Add(new ParameterInfo(parameter));
            }

            Parameters = parameters.ToArray();
        }

        public string Text { get; }

        public ParameterInfo[] Parameters { get; }
    }

    // used for tracking commands
    public class ParameterInfo
    {
        public ParameterInfo(IDbDataParameter parameter)
        {
            Name = parameter.ParameterName;
            Value = parameter.Value;
            DbType = parameter.DbType;
            Size = parameter.Size;
        }

        public string Name { get; }

        public object? Value { get; }

        public DbType DbType { get; }

        public int Size { get; }
    }

    /// <inheritdoc cref="Database.ExecuteScalar{T}(string,object[])" />
    public new T ExecuteScalar<T>(string sql, params object[] args)
        => ExecuteScalar<T>(new Sql(sql, args));

    /// <inheritdoc cref="Database.ExecuteScalar{T}(sql)" />
    public new T ExecuteScalar<T>(Sql sql)
        => ExecuteScalar<T>(sql.SQL, CommandType.Text, sql.Arguments);

    /// <inheritdoc cref="Database.ExecuteScalar{T}(string,CommandType,object[])" />
    /// <remarks>
    ///     Be nice if handled upstream <a href="https://github.com/schotime/NPoco/issues/653">GH issue</a>
    /// </remarks>
    public new T ExecuteScalar<T>(string sql, CommandType commandType, params object[] args)
    {
        if (SqlContext.SqlSyntax.ScalarMappers == null)
        {
            return base.ExecuteScalar<T>(sql, commandType, args);
        }

        if (!SqlContext.SqlSyntax.ScalarMappers.TryGetValue(typeof(T), out IScalarMapper? mapper))
        {
            return base.ExecuteScalar<T>(sql, commandType, args);
        }

        var result = base.ExecuteScalar<object>(sql, commandType, args);
        return (T)mapper.Map(result);
    }
}
