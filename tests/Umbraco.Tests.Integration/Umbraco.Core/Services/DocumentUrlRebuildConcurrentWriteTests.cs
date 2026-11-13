using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Verifies that the document URL and URL alias rebuilds succeed when another connection commits a write while they run.
/// </summary>
/// <remarks>
/// The harness's shared-cache in-memory SQLite database uses table locks rather than WAL snapshots, so it cannot
/// produce the conflict these tests are about. The fixture copies the test database to a file in WAL mode - the way
/// Umbraco creates SQLite databases - and runs against that.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class DocumentUrlRebuildConcurrentWriteTests : UmbracoIntegrationTestWithContent
{
    private static readonly ConcurrentWriter _concurrentWriter = new();

    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    private IDocumentUrlAliasService DocumentUrlAliasService => GetRequiredService<IDocumentUrlAliasService>();

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    public enum WritePoint
    {
        /// <summary>
        /// After the service has read whether a rebuild is needed, and before the rebuild starts.
        /// </summary>
        AfterRebuildCheck,

        /// <summary>
        /// Part-way through the rebuild, after it has read the data it rebuilds from.
        /// </summary>
        DuringRebuild,
    }

    [SetUp]
    public void SkipUnlessSqlite()
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            Assert.Ignore("The conflict under test is specific to SQLite's WAL snapshot isolation.");
        }
    }

    [TearDown]
    public void ResetConcurrentWriter() => _concurrentWriter.Reset();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton(_concurrentWriter);

        builder.Services.AddUnique<IUmbracoDatabaseFactory>(services =>
            new WalFileDatabaseFactory(services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>().Create()));

        builder.Services.AddUnique<IKeyValueService>(services =>
            new WriteOnRebuildCheckKeyValueService(
                ActivatorUtilities.CreateInstance<KeyValueService>(services),
                _concurrentWriter));

        builder.Services.AddUnique<IDocumentUrlAliasRepository>(services =>
            new WriteDuringRebuildDocumentUrlAliasRepository(
                ActivatorUtilities.CreateInstance<DocumentUrlAliasRepository>(services),
                _concurrentWriter));

        builder.UrlSegmentProviders().Insert<WriteDuringRebuildUrlSegmentProvider>();
    }

    [TestCase(WritePoint.AfterRebuildCheck)]
    [TestCase(WritePoint.DuringRebuild)]
    public async Task Document_Url_Rebuild_Succeeds_When_Another_Connection_Commits_A_Write_While_It_Runs(WritePoint writePoint)
    {
        KeyValueService.SetValue(global::Umbraco.Cms.Core.Services.DocumentUrlService.RebuildKey, "stale");
        _concurrentWriter.Arm(writePoint, global::Umbraco.Cms.Core.Services.DocumentUrlService.RebuildKey);

        await DocumentUrlService.InitAsync(false, CancellationToken.None);

        await AssertConcurrentWriteCommitted();
        Assert.AreNotEqual("stale", KeyValueService.GetValue(global::Umbraco.Cms.Core.Services.DocumentUrlService.RebuildKey));
        Assert.IsNotNull(DocumentUrlService.GetUrlSegment(Textpage.Key, "en-US", isDraft: true));
    }

    [TestCase(WritePoint.AfterRebuildCheck)]
    [TestCase(WritePoint.DuringRebuild)]
    public async Task Document_Url_Alias_Rebuild_Succeeds_When_Another_Connection_Commits_A_Write_While_It_Runs(WritePoint writePoint)
    {
        KeyValueService.SetValue(global::Umbraco.Cms.Core.Services.DocumentUrlAliasService.RebuildKey, "stale");
        _concurrentWriter.Arm(writePoint, global::Umbraco.Cms.Core.Services.DocumentUrlAliasService.RebuildKey);

        await DocumentUrlAliasService.InitAsync(false, CancellationToken.None);

        await AssertConcurrentWriteCommitted();
        Assert.AreNotEqual("stale", KeyValueService.GetValue(global::Umbraco.Cms.Core.Services.DocumentUrlAliasService.RebuildKey));
    }

    private async Task AssertConcurrentWriteCommitted()
    {
        Assert.IsNotNull(_concurrentWriter.Write, "The concurrent write was not triggered, so the test proves nothing.");
        await _concurrentWriter.Write!;
        Assert.AreEqual(_concurrentWriter.WrittenValue, KeyValueService.GetValue(ConcurrentWriter.Key));
    }

    /// <summary>
    /// Commits a write on another connection, simulating a background writer such as the search indexer.
    /// </summary>
    internal sealed class ConcurrentWriter
    {
        public const string Key = "DocumentUrlRebuildConcurrentWriteTests";

        // Long enough for an unblocked write to commit; short enough that a write blocked behind the rebuild's write
        // lock gives up waiting well within the lock timeout, letting the rebuild finish and release the lock.
        private static readonly TimeSpan _waitForCommit = TimeSpan.FromMilliseconds(500);

        private WritePoint? _armedPoint;
        private string? _rebuildKey;

        public Task? Write { get; private set; }

        public string? WrittenValue { get; private set; }

        public IKeyValueService? KeyValueService { get; set; }

        public void Arm(WritePoint point, string rebuildKey)
        {
            _armedPoint = point;
            _rebuildKey = rebuildKey;
        }

        public void Reset()
        {
            _armedPoint = null;
            _rebuildKey = null;
            Write = null;
            WrittenValue = null;
        }

        public void OnRebuildKeyRead(string key)
        {
            if (_armedPoint is WritePoint.AfterRebuildCheck && key == _rebuildKey)
            {
                CommitFromAnotherConnection();
            }
        }

        public void OnRebuildRead()
        {
            if (_armedPoint is WritePoint.DuringRebuild)
            {
                CommitFromAnotherConnection();
            }
        }

        private void CommitFromAnotherConnection()
        {
            _armedPoint = null;
            IKeyValueService keyValueService = KeyValueService
                ?? throw new InvalidOperationException("The concurrent writer has not been given a key value service.");

            var value = Guid.NewGuid().ToString();
            WrittenValue = value;

            // Don't flow the ambient scope: the write must run in its own scope, on its own connection.
            using (ExecutionContext.SuppressFlow())
            {
                Write = Task.Run(() => keyValueService.SetValue(Key, value));
            }

            Write.Wait(_waitForCommit);
        }
    }

    private sealed class WriteOnRebuildCheckKeyValueService : IKeyValueService
    {
        private readonly IKeyValueService _inner;
        private readonly ConcurrentWriter _concurrentWriter;

        public WriteOnRebuildCheckKeyValueService(IKeyValueService inner, ConcurrentWriter concurrentWriter)
        {
            _inner = inner;
            _concurrentWriter = concurrentWriter;
            _concurrentWriter.KeyValueService = inner;
        }

        public string? GetValue(string key)
        {
            var value = _inner.GetValue(key);
            _concurrentWriter.OnRebuildKeyRead(key);
            return value;
        }

        public IReadOnlyDictionary<string, string?>? FindByKeyPrefix(string keyPrefix) => _inner.FindByKeyPrefix(keyPrefix);

        public void SetValue(string key, string value) => _inner.SetValue(key, value);

        public void SetValue(string key, string originValue, string newValue) => _inner.SetValue(key, originValue, newValue);

        public bool TrySetValue(string key, string originValue, string newValue) => _inner.TrySetValue(key, originValue, newValue);
    }

    private sealed class WriteDuringRebuildDocumentUrlAliasRepository : IDocumentUrlAliasRepository
    {
        private readonly IDocumentUrlAliasRepository _inner;
        private readonly ConcurrentWriter _concurrentWriter;

        public WriteDuringRebuildDocumentUrlAliasRepository(IDocumentUrlAliasRepository inner, ConcurrentWriter concurrentWriter)
        {
            _inner = inner;
            _concurrentWriter = concurrentWriter;
        }

        public IEnumerable<DocumentUrlAliasRaw> GetAllDocumentUrlAliases()
        {
            var aliases = _inner.GetAllDocumentUrlAliases().ToList();
            _concurrentWriter.OnRebuildRead();
            return aliases;
        }

        public void Save(IEnumerable<PublishedDocumentUrlAlias> aliases) => _inner.Save(aliases);

        public IEnumerable<PublishedDocumentUrlAlias> GetAll() => _inner.GetAll();

        public void DeleteByDocumentKey(IEnumerable<Guid> documentKeys) => _inner.DeleteByDocumentKey(documentKeys);

        public void DeleteAll() => _inner.DeleteAll();
    }

    /// <summary>
    /// Fires the concurrent write the first time a URL segment is requested, which the rebuild does only after reading
    /// the documents it rebuilds; defers to the default provider for the segment itself.
    /// </summary>
    private sealed class WriteDuringRebuildUrlSegmentProvider : IUrlSegmentProvider
    {
        private readonly ConcurrentWriter _concurrentWriter;

        public WriteDuringRebuildUrlSegmentProvider(ConcurrentWriter concurrentWriter) => _concurrentWriter = concurrentWriter;

        public bool AllowAdditionalSegments => true;

        public string? GetUrlSegment(IContentBase content, string? culture = null)
        {
            _concurrentWriter.OnRebuildRead();
            return null;
        }
    }

    /// <summary>
    /// Moves the test database to a file in WAL mode when the harness configures the connection.
    /// </summary>
    private sealed class WalFileDatabaseFactory : IUmbracoDatabaseFactory
    {
        private readonly IUmbracoDatabaseFactory _inner;
        private string? _filePath;

        public WalFileDatabaseFactory(IUmbracoDatabaseFactory inner) => _inner = inner;

        public bool Configured => _inner.Configured;

        public bool Initialized => _inner.Initialized;

        public string? ConnectionString => _inner.ConnectionString;

        public string? ProviderName => _inner.ProviderName;

        public bool CanConnect => _inner.CanConnect;

        public ISqlContext SqlContext => _inner.SqlContext;

        public IBulkSqlInsertProvider? BulkSqlInsertProvider => _inner.BulkSqlInsertProvider;

        public IUmbracoDatabase CreateDatabase() => _inner.CreateDatabase();

        public void ConfigureForUpgrade() => _inner.ConfigureForUpgrade();

        public void Configure(ConnectionStrings umbracoConnectionString)
        {
            _filePath = Path.Combine(Path.GetTempPath(), $"umbraco-{Guid.NewGuid():N}.sqlite.db");
            var fileConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _filePath,
                ForeignKeys = true,
                Pooling = false,
            }.ConnectionString;

            using (var source = new SqliteConnection(umbracoConnectionString.ConnectionString))
            using (var destination = new SqliteConnection(fileConnectionString))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);

                using SqliteCommand command = destination.CreateCommand();
                command.CommandText = "PRAGMA journal_mode = wal;";
                command.ExecuteNonQuery();
            }

            _inner.Configure(new ConnectionStrings
            {
                ConnectionString = fileConnectionString,
                ProviderName = umbracoConnectionString.ProviderName,
            });
        }

        public void Dispose()
        {
            _inner.Dispose();
            if (_filePath is null)
            {
                return;
            }

            SqliteConnection.ClearAllPools();
            foreach (var path in new[] { _filePath, _filePath + "-wal", _filePath + "-shm" })
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
