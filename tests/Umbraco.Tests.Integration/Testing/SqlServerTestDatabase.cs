using System.Collections.Concurrent;
using System.Diagnostics;
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

    public SqlServerTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

        _settings = settings;
        _snapshotDir = Path.Combine(settings.FilesPath, "snapshots");

        var counter = 0;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => TestDatabaseInformation.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", false, _settings.SQLServerMasterConnectionString));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => TestDatabaseInformation.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", true, _settings.SQLServerMasterConnectionString));

        _testDatabases = schema.Concat(empty).ToList();
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDatabaseInformation>();
        _readySchemaQueue = new BlockingCollection<TestDatabaseInformation>();
        _readyEmptyQueue = new BlockingCollection<TestDatabaseInformation>();

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

    private void CreateDatabase(TestDatabaseInformation meta)
    {
        Drop(meta);

        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();

            // Delete leftover .mdf/.ldf files from previously detached (but not dropped) databases
            var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);
            File.Delete(Path.Combine(defaultDataPath, $"{meta.Name}.mdf"));
            File.Delete(Path.Combine(defaultLogPath, $"{meta.Name}_log.ldf"));

            using (var command = connection.CreateCommand())
            {
                SetCommand(command, $@"
                    CREATE DATABASE {LocalDb.QuotedName(meta.Name)};
                    ALTER DATABASE {LocalDb.QuotedName(meta.Name)} SET RECOVERY SIMPLE;
                ");
                command.ExecuteNonQuery();
            }
        }
    }

    private void Drop(TestDatabaseInformation meta) => DropByName(meta.Name);

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

                var sql = KillAllUsersInDb(name);
                SetCommand(command, sql);
                command.ExecuteNonQuery();

                SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    ///     Generates T-SQL that kills all sessions connected to the specified database.
    ///     Uses a cursor approach instead of <c>SET SINGLE_USER WITH ROLLBACK IMMEDIATE</c>
    ///     to avoid deadlocks when multiple connections are active.
    /// </summary>
    private static string KillAllUsersInDb(string databaseName) =>
        $"""
         declare @sessions int, @spid int, @proc nvarchar(50);
         declare cur cursor for select spid from sys.sysprocesses where db_name(dbid) = '{databaseName}';
         select @sessions = spid from sys.sysprocesses where db_name(dbid) = '{databaseName}';
         begin try
             open cur;
             fetch next from cur into @spid;
             while @@FETCH_STATUS = 0
             begin
                 set @proc = N'kill ' + cast(@spid as nvarchar(20));
                 exec sp_executesql @proc;
                 fetch next from cur into @spid;
             end
         end try begin catch end catch;
         begin try close cur;
         end try begin catch end catch;
         deallocate cur;
         select @sessions;
         """;

    #region ISnapshotableTestDatabase

    /// <inheritdoc />
    public bool HasSnapshot(string snapshotKey)
    {
        var seenPath = _snapshotPaths.ContainsKey(snapshotKey);
        var fileExists = seenPath && File.Exists(_snapshotPaths[snapshotKey]);
        if (seenPath && !fileExists)
        {
            _snapshotPaths.Remove(snapshotKey, out _);
        }

        return fileExists;
    }

    /// <inheritdoc />
    public void CreateSnapshot(string snapshotKey, TestDatabaseInformation sourceMeta)
    {
        Directory.CreateDirectory(_snapshotDir);

        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();

        // Get default data/log directories from the server
        var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);

        var dataFilePath = Path.Combine(defaultDataPath, $"{sourceMeta.Name}.mdf").Replace("'", "''");
        var logFilePath = Path.Combine(defaultLogPath, $"{sourceMeta.Name}_log.ldf").Replace("'", "''");

        var cloneDataFilePath = Path.Combine(_snapshotDir, $"{sourceMeta.Name}.mdf");
        var cloneLogFilePath = Path.Combine(_snapshotDir, $"{sourceMeta.Name}_log.ldf");

        using var cmd = connection.CreateCommand();

        // Detach the source database so we can copy its files
        Retry(10, () =>
        {
            cmd.CommandText = KillAllUsersInDb(sourceMeta.Name);
            cmd.ExecuteScalar();

            cmd.CommandText = $"exec sp_detach_db N'{sourceMeta.Name}', true, true";
            cmd.ExecuteNonQuery();
        });

        // Copy .mdf and .ldf to the snapshot directory
        File.Copy(dataFilePath, cloneDataFilePath, true);
        File.Copy(logFilePath, cloneLogFilePath, true);

        // Reattach the source database
        cmd.CommandText = $"exec sp_attach_db N'{sourceMeta.Name}', N'{dataFilePath}', N'{logFilePath}'";
        cmd.ExecuteNonQuery();

        _snapshotPaths[snapshotKey] = cloneDataFilePath;
    }

    /// <inheritdoc />
    public TestDatabaseInformation AttachFromSnapshot(string snapshotKey)
    {
        if (!_snapshotPaths.TryGetValue(snapshotKey, out var snapshotDataPath))
        {
            throw new InvalidOperationException($"No snapshot found with key '{snapshotKey}'.");
        }

        // Reuse a pool database instead of creating a new one
        var meta = _readySchemaQueue.Take();
        var dbName = meta.Name;

        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();

        // Get default data/log directories from the server
        var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);

        var dataFilePath = Path.Combine(defaultDataPath, $"{dbName}.mdf").Replace("'", "''");
        var logFilePath = Path.Combine(defaultLogPath, $"{dbName}_log.ldf").Replace("'", "''");

        var snapshotLogPath = snapshotDataPath.Replace(".mdf", "_log.ldf");

        using var cmd = connection.CreateCommand();

        // Detach the pool database so we can overwrite its files
        Retry(10, () =>
        {
            cmd.CommandText = KillAllUsersInDb(dbName);
            cmd.ExecuteScalar();

            cmd.CommandText = $"exec sp_detach_db N'{dbName}', true, true";
            cmd.ExecuteNonQuery();
        });

        // Overwrite the pool database's files with the snapshot
        File.Copy(snapshotDataPath, dataFilePath, true);
        File.Copy(snapshotLogPath, logFilePath, true);

        // Reattach with the pool database's name
        cmd.CommandText = $@"
            exec sp_attach_db N'{dbName}', N'{dataFilePath}', N'{logFilePath}';
            ALTER DATABASE [{dbName}] SET MULTI_USER;
        ";
        cmd.ExecuteNonQuery();

        return meta;
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

        var stopwatch = Stopwatch.StartNew();

        _prepareQueue.CompleteAdding();
        while (_prepareQueue.TryTake(out _, 10) && stopwatch.ElapsedMilliseconds < 5000)
        {
        }

        stopwatch.Restart();
        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _, 10) && stopwatch.ElapsedMilliseconds < 5000)
        {
        }

        stopwatch.Restart();
        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _, 10) && stopwatch.ElapsedMilliseconds < 5000)
        {
        }

        // Drop pool databases
        Parallel.ForEach(_testDatabases, Drop);

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

        _snapshotPaths.Clear();
    }
}
