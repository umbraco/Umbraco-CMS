// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer;

// ReSharper disable ConvertToUsingDeclaration
namespace Umbraco.Cms.Tests.Integration.Testing;

/// <remarks>
///     It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
/// </remarks>
public class SqlServerTestDatabase : SqlServerBaseTestDatabase, ITestDatabase, ISnapshotableTestDatabase
{
    public const string DatabaseName = "UmbracoTests";
    private readonly TestDatabaseSettings _settings;
    private readonly string _snapshotDir;
    private readonly ConcurrentDictionary<string, string> _snapshotPaths = new();
    private readonly ConcurrentBag<string> _snapshotRestoredDatabases = new();
    private int _snapshotCounter;

    public SqlServerTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

        _settings = settings;
        _snapshotDir = Path.Combine(settings.FilesPath, "snapshots");

        var counter = 0;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", false, _settings.SQLServerMasterConnectionString));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => TestDbMeta.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", true, _settings.SQLServerMasterConnectionString));

        _testDatabases = schema.Concat(empty).ToList();
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDbMeta>();
        _readySchemaQueue = new BlockingCollection<TestDbMeta>();
        _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

        foreach (var meta in _testDatabases)
        {
            CreateDatabase(meta);
            _prepareQueue.Add(meta);
        }

        for (var i = 0; i < _settings.PrepareThreadCount; i++)
        {
            var thread = new Thread(PrepareDatabase);
            thread.Start();
        }
    }

    private void CreateDatabase(TestDbMeta meta)
    {
        Drop(meta);

        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, $@"CREATE DATABASE {LocalDb.QuotedName(meta.Name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    private void Drop(TestDbMeta meta) => DropByName(meta.Name);

    private void DropByName(string name)
    {
        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, "select count(1) from sys.databases where name = @0", name);
                var records = (int)command.ExecuteScalar();
                if (records == 0)
                {
                    return;
                }

                var sql = $@"
                        ALTER DATABASE {LocalDb.QuotedName(name)}
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE";
                SetCommand(command, sql);
                command.ExecuteNonQuery();

                SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    #region ISnapshotableTestDatabase

    /// <inheritdoc />
    public bool HasSnapshot(string snapshotKey) => _snapshotPaths.ContainsKey(snapshotKey);

    /// <inheritdoc />
    public void CreateSnapshot(string snapshotKey, TestDbMeta sourceMeta)
    {
        Directory.CreateDirectory(_snapshotDir);
        var backupPath = Path.Combine(_snapshotDir, $"{snapshotKey}.bak");

        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();

        // BACKUP DATABASE cannot use SQL parameters for database name or file path.
        // Names are internally generated (not user input), matching existing DDL patterns.
        cmd.CommandText = $@"
            BACKUP DATABASE {LocalDb.QuotedName(sourceMeta.Name)}
            TO DISK = N'{backupPath.Replace("'", "''")}'
            WITH INIT, COMPRESSION";
        cmd.ExecuteNonQuery();

        _snapshotPaths[snapshotKey] = backupPath;
    }

    /// <inheritdoc />
    public TestDbMeta AttachFromSnapshot(string snapshotKey)
    {
        if (!_snapshotPaths.TryGetValue(snapshotKey, out var backupPath))
        {
            throw new InvalidOperationException($"No snapshot found with key '{snapshotKey}'.");
        }

        var dbName = $"{DatabaseName}-Snap-{Interlocked.Increment(ref _snapshotCounter)}";
        var meta = TestDbMeta.CreateWithMasterConnectionString(dbName, false, _settings.SQLServerMasterConnectionString);

        _snapshotRestoredDatabases.Add(dbName);

        // Drop if a database with this name already exists
        DropByName(dbName);

        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();

        // Get logical file names from the backup
        var (dataLogicalName, logLogicalName) = GetLogicalFileNames(connection, backupPath);

        // Get default data/log directories from the server
        var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);

        // RESTORE DATABASE cannot use SQL parameters for identifiers or file paths.
        var escapedBackupPath = backupPath.Replace("'", "''");
        var dataFilePath = Path.Combine(defaultDataPath, $"{dbName}.mdf").Replace("'", "''");
        var logFilePath = Path.Combine(defaultLogPath, $"{dbName}_log.ldf").Replace("'", "''");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            RESTORE DATABASE {LocalDb.QuotedName(dbName)}
            FROM DISK = N'{escapedBackupPath}'
            WITH MOVE N'{dataLogicalName.Replace("'", "''")}' TO N'{dataFilePath}',
                 MOVE N'{logLogicalName.Replace("'", "''")}' TO N'{logFilePath}',
                 REPLACE";
        cmd.ExecuteNonQuery();

        return meta;
    }

    private static (string DataLogical, string LogLogical) GetLogicalFileNames(
        SqlConnection connection, string backupPath)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"RESTORE FILELISTONLY FROM DISK = N'{backupPath.Replace("'", "''")}'";

        string dataName = null;
        string logName = null;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var type = reader["Type"].ToString();
            var logicalName = reader["LogicalName"].ToString();
            if (type == "D")
            {
                dataName = logicalName;
            }
            else if (type == "L")
            {
                logName = logicalName;
            }
        }

        return (dataName ?? throw new InvalidOperationException("No data file found in backup."),
                logName ?? throw new InvalidOperationException("No log file found in backup."));
    }

    private static (string DataPath, string LogPath) GetDefaultPaths(SqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath,
                   SERVERPROPERTY('InstanceDefaultLogPath') AS LogPath";

        using var reader = cmd.ExecuteReader();
        reader.Read();
        return (reader["DataPath"].ToString()!, reader["LogPath"].ToString()!);
    }

    #endregion

    public override void TearDown()
    {
        if (_prepareQueue == null)
        {
            return;
        }

        _prepareQueue.CompleteAdding();
        while (_prepareQueue.TryTake(out _))
        {
        }

        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _))
        {
        }

        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _))
        {
        }

        // Drop pool databases
        Parallel.ForEach(_testDatabases, Drop);

        // Drop snapshot-restored databases
        foreach (var name in _snapshotRestoredDatabases)
        {
            DropByName(name);
        }

        // Clean up snapshot files
        if (Directory.Exists(_snapshotDir))
        {
            try
            {
                Directory.Delete(_snapshotDir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup
            }
        }
    }
}
