using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Manages LocalDB databases.
/// </summary>
/// <remarks>
///     <para>
///         Latest version is SQL Server 2016 Express LocalDB,
///         see https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-2016-express-localdb
///         which can be installed by downloading the Express installer from
///         https://www.microsoft.com/en-us/sql-server/sql-server-downloads
///         (about 5MB) then select 'download media' to download SqlLocalDB.msi (about 44MB), which you can execute. This
///         installs
///         LocalDB only. Though you probably want to install the full Express. You may also want to install SQL Server
///         Management
///         Studio which can be used to connect to LocalDB databases.
///     </para>
///     <para>See also https://github.com/ritterim/automation-sql which is a somewhat simpler version of this.</para>
/// </remarks>
public class LocalDb
{
    private string? _exe;
    private bool _hasVersion;
    private int _version;

    #region Availability & Version

    /// <summary>
    ///     Gets the LocalDb installed version.
    /// </summary>
    /// <remarks>
    ///     If more than one version is installed, returns the highest available. Returns
    ///     the major version as an integer e.g. 11, 12...
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    public int Version
    {
        get
        {
            EnsureVersion();
            if (_version <= 0)
            {
                throw new InvalidOperationException("LocalDb is not available.");
            }

            return _version;
        }
    }

    /// <summary>
    ///     Ensures that the LocalDb version is detected.
    /// </summary>
    private void EnsureVersion()
    {
        if (_hasVersion)
        {
            return;
        }

        DetectVersion();
        _hasVersion = true;
    }

    /// <summary>
    ///     Gets a value indicating whether LocalDb is available.
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            EnsureVersion();
            return _version > 0;
        }
    }

    /// <summary>
    ///     Ensures that LocalDb is available.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    private void EnsureAvailable()
    {
        if (IsAvailable == false)
        {
            throw new InvalidOperationException("LocalDb is not available.");
        }
    }

    /// <summary>
    ///     Detects LocalDb installed version.
    /// </summary>
    /// <remarks>If more than one version is installed, the highest available is detected.</remarks>
    private void DetectVersion()
    {
        _hasVersion = true;
        _version = -1;
        _exe = null;

        var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");

        // MS SQL Server installs in e.g. "C:\Program Files\Microsoft SQL Server", so
        // we want to detect it in "%ProgramFiles%\Microsoft SQL Server" - however, if
        // Umbraco runs as a 32bits process (e.g. IISExpress configured as 32bits)
        // on a 64bits system, %ProgramFiles% will point to "C:\Program Files (x86)"
        // and SQL Server cannot be found. But then, %ProgramW6432% will point to
        // the original "C:\Program Files". Using it to fix the path.
        // see also: MSDN doc for WOW64 implementation
        //
        var programW6432 = Environment.GetEnvironmentVariable("ProgramW6432");
        if (string.IsNullOrWhiteSpace(programW6432) == false && programW6432 != programFiles)
        {
            programFiles = programW6432;
        }

        if (string.IsNullOrWhiteSpace(programFiles))
        {
            return;
        }

        // detect 15, 14, 13, 12, 11
        for (var i = 15; i > 10; i--)
        {
            var exe = Path.Combine(programFiles, $@"Microsoft SQL Server\{i}0\Tools\Binn\SqlLocalDB.exe");
            if (File.Exists(exe) == false)
            {
                continue;
            }

            _version = i;
            _exe = exe;
            break;
        }
    }

    #endregion

    #region Instances

    /// <summary>
    ///     Gets the name of existing LocalDb instances.
    /// </summary>
    /// <returns>The name of existing LocalDb instances.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    public string[]? GetInstances()
    {
        EnsureAvailable();
        var rc = ExecuteSqlLocalDb("i", out var output, out var error); // info
        if (rc != 0 || error != string.Empty)
        {
            return null;
        }

        return output.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    ///     Gets a value indicating whether a LocalDb instance exists.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>A value indicating whether a LocalDb instance with the specified name exists.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    public bool InstanceExists(string instanceName)
    {
        EnsureAvailable();
        var instances = GetInstances();
        return instances != null && instances.Contains(instanceName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Creates a LocalDb instance.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>A value indicating whether the instance was created without errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    public bool CreateInstance(string instanceName)
    {
        EnsureAvailable();
        return ExecuteSqlLocalDb($"c \"{instanceName}\"", out _, out var error) == 0 && error == string.Empty;
    }

    /// <summary>
    ///     Drops a LocalDb instance.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>A value indicating whether the instance was dropped without errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    /// <remarks>
    ///     When an instance is dropped all the attached database files are deleted.
    ///     Successful if the instance does not exist.
    /// </remarks>
    public bool DropInstance(string instanceName)
    {
        EnsureAvailable();
        Instance? instance = GetInstance(instanceName);
        if (instance == null)
        {
            return true;
        }

        instance.DropDatabases(); // else the files remain

        // -i force NOWAIT, -k kills
        return ExecuteSqlLocalDb($"p \"{instanceName}\" -i", out _, out var error) == 0 && error == string.Empty
            && ExecuteSqlLocalDb($"d \"{instanceName}\"", out _, out error) == 0 && error == string.Empty;
    }

    /// <summary>
    ///     Stops a LocalDb instance.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>A value indicating whether the instance was stopped without errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    /// <remarks>
    ///     Successful if the instance does not exist.
    /// </remarks>
    public bool StopInstance(string instanceName)
    {
        EnsureAvailable();
        if (InstanceExists(instanceName) == false)
        {
            return true;
        }

        // -i force NOWAIT, -k kills
        return ExecuteSqlLocalDb($"p \"{instanceName}\" -i", out _, out var error) == 0 && error == string.Empty;
    }

    /// <summary>
    ///     Stops a LocalDb instance.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>A value indicating whether the instance was started without errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    /// <remarks>
    ///     Failed if the instance does not exist.
    /// </remarks>
    public bool StartInstance(string instanceName)
    {
        EnsureAvailable();
        if (InstanceExists(instanceName) == false)
        {
            return false;
        }

        return ExecuteSqlLocalDb($"s \"{instanceName}\"", out _, out var error) == 0 && error == string.Empty;
    }

    /// <summary>
    ///     Gets a LocalDb instance.
    /// </summary>
    /// <param name="instanceName">The name of the instance.</param>
    /// <returns>The instance with the specified name if it exists, otherwise null.</returns>
    /// <exception cref="InvalidOperationException">Thrown when LocalDb is not available.</exception>
    public Instance? GetInstance(string instanceName)
    {
        EnsureAvailable();
        return InstanceExists(instanceName) ? new Instance(instanceName) : null;
    }

    #endregion

    #region Databases

    /// <summary>
    ///     Represents a LocalDb instance.
    /// </summary>
    /// <remarks>
    ///     LocalDb is assumed to be available, and the instance is assumed to exist.
    /// </remarks>
    public class Instance
    {
        private readonly string _masterCstr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Instance" /> class.
        /// </summary>
        /// <param name="instanceName"></param>
        public Instance(string instanceName)
        {
            InstanceName = instanceName;
            _masterCstr = $@"Server=(localdb)\{instanceName};Integrated Security=True;";
        }

        /// <summary>
        ///     Gets the name of the instance.
        /// </summary>
        public string InstanceName { get; }

        public static string GetConnectionString(string instanceName, string databaseName) =>
            $@"Server=(localdb)\{instanceName};Integrated Security=True;Database={databaseName};";

        /// <summary>
        ///     Gets a LocalDb connection string.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>The connection string for the specified database.</returns>
        /// <remarks>
        ///     The database should exist in the LocalDb instance.
        /// </remarks>
        public string GetConnectionString(string databaseName) => _masterCstr + $@"Database={databaseName};";

        /// <summary>
        ///     Gets a LocalDb connection string for an attached database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="filesPath">The directory containing database files.</param>
        /// <returns>The connection string for the specified database.</returns>
        /// <remarks>
        ///     The database should not exist in the LocalDb instance.
        ///     It will be attached with its name being its MDF filename (full path), uppercased, when
        ///     the first connection is opened, and remain attached until explicitly detached.
        /// </remarks>
        public string GetAttachedConnectionString(string databaseName, string filesPath)
        {
            GetDatabaseFiles(databaseName, filesPath, out _, out _, out _, out var mdfFilename, out _);

            return _masterCstr + $@"AttachDbFileName='{mdfFilename}';";
        }

        /// <summary>
        ///     Gets the name of existing databases.
        /// </summary>
        /// <returns>The name of existing databases.</returns>
        public string[] GetDatabases()
        {
            var userDatabases = new List<string>();

            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var databases = new Dictionary<string, string>();

                SetCommand(cmd, @"
                        SELECT name, filename FROM sys.sysdatabases");

                using (SqlDataReader? reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases[reader.GetString(0)] = reader.GetString(1);
                    }
                }

                foreach (KeyValuePair<string, string> database in databases)
                {
                    var dbname = database.Key;

                    if (dbname == "master" || dbname == "tempdb" || dbname == "model" || dbname == "msdb")
                    {
                        continue;
                    }

                    // TODO: shall we deal with stale databases?
                    // TODO: is it always ok to assume file names?
                    //var mdf = database.Value;
                    //var ldf = mdf.Replace(".mdf", "_log.ldf");
                    //if (staleOnly && File.Exists(mdf) && File.Exists(ldf))
                    //    continue;

                    //ExecuteDropDatabase(cmd, dbname, mdf, ldf);
                    //count++;

                    userDatabases.Add(dbname);
                }
            }

            return userDatabases.ToArray();
        }

        /// <summary>
        ///     Gets a value indicating whether a database exists.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A value indicating whether a database with the specified name exists.</returns>
        /// <remarks>
        ///     A database exists if it is registered in the instance, and its files exist. If the database
        ///     is registered but some of its files are missing, the database is dropped.
        /// </remarks>
        public bool DatabaseExists(string databaseName)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var mdf = GetDatabase(cmd, databaseName);
                if (mdf == null)
                {
                    return false;
                }

                // it can exist, even though its files have been deleted
                // if files exist assume all is ok (should we try to connect?)
                var ldf = GetLogFilename(mdf);
                if (File.Exists(mdf) && File.Exists(ldf))
                {
                    return true;
                }

                ExecuteDropDatabase(cmd, databaseName, mdf, ldf);
            }

            return false;
        }

        /// <summary>
        ///     Creates a new database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="filesPath">The directory containing database files.</param>
        /// <returns>A value indicating whether the database was created without errors.</returns>
        /// <remarks>
        ///     Failed if a database with the specified name already exists in the instance,
        ///     or if the database files already exist in the specified directory.
        /// </remarks>
        public bool CreateDatabase(string databaseName, string filesPath)
        {
            GetDatabaseFiles(databaseName, filesPath, out var logName, out _, out _, out var mdfFilename,
                out var ldfFilename);

            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var mdf = GetDatabase(cmd, databaseName);
                if (mdf != null)
                {
                    return false;
                }

                // cannot use parameters on CREATE DATABASE
                // ie "CREATE DATABASE @0 ..." does not work
                SetCommand(cmd, $@"
                        CREATE DATABASE {QuotedName(databaseName)}
                            ON (NAME=N{QuotedName(databaseName, '\'')}, FILENAME={QuotedName(mdfFilename, '\'')})
                            LOG ON (NAME=N{QuotedName(logName, '\'')}, FILENAME={QuotedName(ldfFilename, '\'')})");

                var unused = cmd.ExecuteNonQuery();
            }

            return true;
        }

        /// <summary>
        ///     Drops a database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>A value indicating whether the database was dropped without errors.</returns>
        /// <remarks>
        ///     Successful if the database does not exist.
        ///     Deletes the database files.
        /// </remarks>
        public bool DropDatabase(string databaseName)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                SetCommand(cmd, @"
                        SELECT name, filename FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = @0 OR name = @0)",
                    databaseName);

                var mdf = GetDatabase(cmd, databaseName);
                if (mdf == null)
                {
                    return true;
                }

                ExecuteDropDatabase(cmd, databaseName, mdf);
            }

            return true;
        }

        /// <summary>
        ///     Drops stale databases.
        /// </summary>
        /// <returns>The number of databases that were dropped.</returns>
        /// <remarks>
        ///     A database is considered stale when its files cannot be found.
        /// </remarks>
        public int DropStaleDatabases() => DropDatabases(true);

        /// <summary>
        ///     Drops databases.
        /// </summary>
        /// <param name="staleOnly">A value indicating whether to delete only stale database.</param>
        /// <returns>The number of databases that were dropped.</returns>
        /// <remarks>
        ///     A database is considered stale when its files cannot be found.
        /// </remarks>
        public int DropDatabases(bool staleOnly = false)
        {
            var count = 0;
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var databases = new Dictionary<string, string>();

                SetCommand(cmd, @"
                        SELECT name, filename FROM sys.sysdatabases");

                using (SqlDataReader? reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases[reader.GetString(0)] = reader.GetString(1);
                    }
                }

                foreach (KeyValuePair<string, string> database in databases)
                {
                    var dbname = database.Key;

                    if (dbname == "master" || dbname == "tempdb" || dbname == "model" || dbname == "msdb")
                    {
                        continue;
                    }

                    var mdf = database.Value;
                    var ldf = mdf.Replace(".mdf", "_log.ldf");
                    if (staleOnly && File.Exists(mdf) && File.Exists(ldf))
                    {
                        continue;
                    }

                    ExecuteDropDatabase(cmd, dbname, mdf, ldf);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Detaches a database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>The directory containing the database files.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a database with the specified name does not exist.</exception>
        public string? DetachDatabase(string databaseName)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var mdf = GetDatabase(cmd, databaseName);
                if (mdf == null)
                {
                    throw new InvalidOperationException("Database does not exist.");
                }

                DetachDatabase(cmd, databaseName);

                return Path.GetDirectoryName(mdf);
            }
        }

        /// <summary>
        ///     Attaches a database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="filesPath">The directory containing database files.</param>
        /// <exception cref="InvalidOperationException">Thrown when a database with the specified name already exists.</exception>
        public void AttachDatabase(string databaseName, string filesPath)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                var mdf = GetDatabase(cmd, databaseName);
                if (mdf != null)
                {
                    throw new InvalidOperationException("Database already exists.");
                }

                AttachDatabase(cmd, databaseName, filesPath);
            }
        }

        /// <summary>
        ///     Gets the file names of a database.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="mdfName">The MDF logical name.</param>
        /// <param name="ldfName">The LDF logical name.</param>
        /// <param name="mdfFilename">The MDF filename.</param>
        /// <param name="ldfFilename">The LDF filename.</param>
        public void GetFilenames(string databaseName,
            out string? mdfName, out string? ldfName,
            out string? mdfFilename, out string? ldfFilename)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                GetFilenames(cmd, databaseName, out mdfName, out ldfName, out mdfFilename, out ldfFilename);
            }
        }

        /// <summary>
        ///     Kills all existing connections.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        public void KillConnections(string databaseName)
        {
            using (var conn = new SqlConnection(_masterCstr))
            using (SqlCommand? cmd = conn.CreateCommand())
            {
                conn.Open();

                SetCommand(cmd, @"
                        DECLARE @sql VARCHAR(MAX);
                        SELECT @sql = COALESCE(@sql,'') + 'kill ' + CONVERT(VARCHAR, SPId) + ';'
                            FROM master.sys.sysprocesses
                            WHERE DBId = DB_ID(@0) AND SPId <> @@SPId;
                        EXEC(@sql);",
                    databaseName);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Gets a database.
        /// </summary>
        /// <param name="cmd">The Sql Command.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <returns>The full filename of the MDF file, if the database exists, otherwise null.</returns>
        private static string? GetDatabase(SqlCommand cmd, string databaseName)
        {
            SetCommand(cmd, @"
                    SELECT name, filename FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = @0 OR name = @0)",
                databaseName);

            string? mdf = null;
            using (SqlDataReader? reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    mdf = reader.GetString(1) ?? string.Empty;
                }

                while (reader.Read())
                {
                }
            }

            return mdf;
        }

        /// <summary>
        ///     Drops a database and its files.
        /// </summary>
        /// <param name="cmd">The Sql command.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="mdf">The name of the database (MDF) file.</param>
        /// <param name="ldf">The name of the log (LDF) file.</param>
        private static void ExecuteDropDatabase(SqlCommand cmd, string databaseName, string mdf, string? ldf = null)
        {
            try
            {
                // cannot use parameters on ALTER DATABASE
                // ie "ALTER DATABASE @0 ..." does not work
                SetCommand(cmd, $@"
                        ALTER DATABASE {QuotedName(databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

                var unused1 = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                if (e.Message.Contains("Unable to open the physical file") &&
                    e.Message.Contains("Operating system error 2:"))
                {
                    // quite probably, the files were missing
                    // yet, it should be possible to drop the database anyways
                    // but we'll have to deal with the files
                }
                else
                {
                    // no idea, throw
                    throw;
                }
            }

            // cannot use parameters on DROP DATABASE
            // ie "DROP DATABASE @0 ..." does not work
            SetCommand(cmd, $@"
                    DROP DATABASE {QuotedName(databaseName)}");

            var unused2 = cmd.ExecuteNonQuery();

            // be absolutely sure
            if (File.Exists(mdf))
            {
                File.Delete(mdf);
            }

            ldf = ldf ?? GetLogFilename(mdf);
            if (File.Exists(ldf))
            {
                File.Delete(ldf);
            }
        }

        /// <summary>
        ///     Gets the log (LDF) filename corresponding to a database (MDF) filename.
        /// </summary>
        /// <param name="mdfFilename">The MDF filename.</param>
        /// <returns></returns>
        private static string GetLogFilename(string mdfFilename)
        {
            if (mdfFilename.EndsWith(".mdf") == false)
            {
                throw new ArgumentException("Not a valid MDF filename (no .mdf extension).", nameof(mdfFilename));
            }

            return mdfFilename.Substring(0, mdfFilename.Length - ".mdf".Length) + "_log.ldf";
        }

        /// <summary>
        ///     Detaches a database.
        /// </summary>
        /// <param name="cmd">The Sql command.</param>
        /// <param name="databaseName">The name of the database.</param>
        private static void DetachDatabase(SqlCommand cmd, string databaseName)
        {
            // cannot use parameters on ALTER DATABASE
            // ie "ALTER DATABASE @0 ..." does not work
            SetCommand(cmd, $@"
                    ALTER DATABASE {QuotedName(databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE");

            var unused1 = cmd.ExecuteNonQuery();

            SetCommand(cmd, @"
                    EXEC sp_detach_db @dbname=@0",
                databaseName);

            var unused2 = cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Attaches a database.
        /// </summary>
        /// <param name="cmd">The Sql command.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="filesPath">The directory containing database files.</param>
        private static void AttachDatabase(SqlCommand cmd, string databaseName, string filesPath)
        {
            GetDatabaseFiles(databaseName, filesPath,
                out var logName, out _, out _, out var mdfFilename, out var ldfFilename);

            // cannot use parameters on CREATE DATABASE
            // ie "CREATE DATABASE @0 ..." does not work
            SetCommand(cmd, $@"
                        CREATE DATABASE {QuotedName(databaseName)}
                            ON (NAME=N{QuotedName(databaseName, '\'')}, FILENAME={QuotedName(mdfFilename, '\'')})
                            LOG ON (NAME=N{QuotedName(logName, '\'')}, FILENAME={QuotedName(ldfFilename, '\'')})
                            FOR ATTACH");

            var unused = cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Sets a database command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="sql">The command text.</param>
        /// <param name="args">The command arguments.</param>
        /// <remarks>
        ///     The command text must refer to arguments as @0, @1... each referring
        ///     to the corresponding position in <paramref name="args" />.
        /// </remarks>
        private static void SetCommand(SqlCommand cmd, string sql, params object[] args)
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.Parameters.Clear();
            for (var i = 0; i < args.Length; i++)
            {
                cmd.Parameters.AddWithValue("@" + i, args[i]);
            }
        }

        /// <summary>
        ///     Gets the file names of a database.
        /// </summary>
        /// <param name="cmd">The Sql command.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="mdfName">The MDF logical name.</param>
        /// <param name="ldfName">The LDF logical name.</param>
        /// <param name="mdfFilename">The MDF filename.</param>
        /// <param name="ldfFilename">The LDF filename.</param>
        private void GetFilenames(SqlCommand cmd, string databaseName,
            out string? mdfName, out string? ldfName,
            out string? mdfFilename, out string? ldfFilename)
        {
            mdfName = ldfName = mdfFilename = ldfFilename = null;

            SetCommand(cmd, @"
                    SELECT DB_NAME(database_id), type_desc, name, physical_name
                    FROM master.sys.master_files
                    WHERE database_id=DB_ID(@0)",
                databaseName);
            using (SqlDataReader? reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(1);
                    if (type == "ROWS")
                    {
                        mdfName = reader.GetString(2);
                        ldfName = reader.GetString(3);
                    }
                    else if (type == "LOG")
                    {
                        ldfName = reader.GetString(2);
                        ldfFilename = reader.GetString(3);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Copy database files.
    /// </summary>
    /// <param name="databaseName">The name of the source database.</param>
    /// <param name="filesPath">The directory containing source database files.</param>
    /// <param name="targetDatabaseName">The name of the target database.</param>
    /// <param name="targetFilesPath">The directory containing target database files.</param>
    /// <param name="sourceExtension">The source database files extension.</param>
    /// <param name="targetExtension">The target database files extension.</param>
    /// <param name="overwrite">A value indicating whether to overwrite the target files.</param>
    /// <param name="delete">A value indicating whether to delete the source files.</param>
    /// <remarks>
    ///     The <paramref name="targetDatabaseName" />, <paramref name="targetFilesPath" />,
    ///     <paramref name="sourceExtension" />
    ///     and <paramref name="targetExtension" /> parameters are optional. If they result in target being identical
    ///     to source, no copy is performed. If <paramref name="delete" /> is false, nothing happens, otherwise the source
    ///     files are deleted.
    ///     If target is not identical to source, files are copied or moved, depending on the value of
    ///     <paramref name="delete" />.
    ///     Extensions are used eg to copy MyDatabase.mdf to MyDatabase.mdf.temp.
    /// </remarks>
    public void CopyDatabaseFiles(string databaseName, string filesPath,
        string? targetDatabaseName = null, string? targetFilesPath = null,
        string? sourceExtension = null, string? targetExtension = null,
        bool overwrite = false, bool delete = false)
    {
        var nop = (targetFilesPath == null || targetFilesPath == filesPath)
                  && (targetDatabaseName == null || targetDatabaseName == databaseName)
                  && ((sourceExtension == null && targetExtension == null) || sourceExtension == targetExtension);
        if (nop && delete == false)
        {
            return;
        }

        GetDatabaseFiles(databaseName, filesPath,
            out _, out _, out _, out var mdfFilename, out var ldfFilename);

        if (sourceExtension != null)
        {
            mdfFilename += "." + sourceExtension;
            ldfFilename += "." + sourceExtension;
        }

        if (nop)
        {
            // delete
            if (File.Exists(mdfFilename))
            {
                File.Delete(mdfFilename);
            }

            if (File.Exists(ldfFilename))
            {
                File.Delete(ldfFilename);
            }
        }
        else
        {
            // copy or copy+delete ie move
            GetDatabaseFiles(targetDatabaseName ?? databaseName, targetFilesPath ?? filesPath,
                out _, out _, out _, out var targetMdfFilename, out var targetLdfFilename);

            if (targetExtension != null)
            {
                targetMdfFilename += "." + targetExtension;
                targetLdfFilename += "." + targetExtension;
            }

            if (delete)
            {
                if (overwrite && File.Exists(targetMdfFilename))
                {
                    File.Delete(targetMdfFilename);
                }

                if (overwrite && File.Exists(targetLdfFilename))
                {
                    File.Delete(targetLdfFilename);
                }

                File.Move(mdfFilename, targetMdfFilename);
                File.Move(ldfFilename, targetLdfFilename);
            }
            else
            {
                File.Copy(mdfFilename, targetMdfFilename, overwrite);
                File.Copy(ldfFilename, targetLdfFilename, overwrite);
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether database files exist.
    /// </summary>
    /// <param name="databaseName">The name of the source database.</param>
    /// <param name="filesPath">The directory containing source database files.</param>
    /// <param name="extension">The database files extension.</param>
    /// <returns>A value indicating whether the database files exist.</returns>
    /// <remarks>
    ///     Extensions are used eg to copy MyDatabase.mdf to MyDatabase.mdf.temp.
    /// </remarks>
    public bool DatabaseFilesExist(string databaseName, string filesPath, string? extension = null)
    {
        GetDatabaseFiles(databaseName, filesPath,
            out _, out _, out _, out var mdfFilename, out var ldfFilename);

        if (extension != null)
        {
            mdfFilename += "." + extension;
            ldfFilename += "." + extension;
        }

        return File.Exists(mdfFilename) && File.Exists(ldfFilename);
    }

    /// <summary>
    ///     Gets the name of the database files.
    /// </summary>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="filesPath">The directory containing database files.</param>
    /// <param name="logName">The name of the log.</param>
    /// <param name="baseFilename">The base filename (the MDF filename without the .mdf extension).</param>
    /// <param name="baseLogFilename">The base log filename (the LDF filename without the .ldf extension).</param>
    /// <param name="mdfFilename">The MDF filename.</param>
    /// <param name="ldfFilename">The LDF filename.</param>
    private static void GetDatabaseFiles(string databaseName, string filesPath,
        out string logName,
        out string baseFilename, out string baseLogFilename,
        out string mdfFilename, out string ldfFilename)
    {
        logName = databaseName + "_log";
        baseFilename = Path.Combine(filesPath, databaseName);
        baseLogFilename = Path.Combine(filesPath, logName);
        mdfFilename = baseFilename + ".mdf";
        ldfFilename = baseFilename + "_log.ldf";
    }

    #endregion

    #region SqlLocalDB

    /// <summary>
    ///     Executes the SqlLocalDB command.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <param name="output">The command standard output.</param>
    /// <param name="error">The command error output.</param>
    /// <returns>The process exit code.</returns>
    /// <remarks>
    ///     Execution is successful if the exit code is zero, and error is empty.
    /// </remarks>
    private int ExecuteSqlLocalDb(string args, out string output, out string error)
    {
        if (_exe == null) // should never happen - we should not execute if not available
        {
            output = string.Empty;
            error = "SqlLocalDB.exe not found";
            return -1;
        }

        using (var p = new Process
               {
                   StartInfo =
                   {
                       UseShellExecute = false,
                       RedirectStandardOutput = true,
                       RedirectStandardError = true,
                       FileName = _exe,
                       Arguments = args,
                       CreateNoWindow = true,
                       WindowStyle = ProcessWindowStyle.Hidden
                   }
               })
        {
            p.Start();
            output = p.StandardOutput.ReadToEnd();
            error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            return p.ExitCode;
        }
    }

    /// <summary>
    ///     Returns a Unicode string with the delimiters added to make the input string a valid SQL Server delimited
    ///     identifier.
    /// </summary>
    /// <param name="name">The name to quote.</param>
    /// <param name="quote">A quote character.</param>
    /// <returns></returns>
    /// <remarks>
    ///     This is a C# implementation of T-SQL QUOTEDNAME.
    ///     <paramref name="quote" /> is optional, it can be '[' (default), ']', '\'' or '"'.
    /// </remarks>
    internal static string QuotedName(string name, char quote = '[')
    {
        switch (quote)
        {
            case '[':
            case ']':
                return "[" + name.Replace("]", "]]") + "]";
            case '\'':
                return "'" + name.Replace("'", "''") + "'";
            case '"':
                return "\"" + name.Replace("\"", "\"\"") + "\"";
            default:
                throw new NotSupportedException("Not a valid quote character.");
        }
    }

    #endregion
}
