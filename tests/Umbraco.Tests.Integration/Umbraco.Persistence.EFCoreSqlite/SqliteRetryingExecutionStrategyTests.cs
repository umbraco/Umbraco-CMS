using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SQLitePCL;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.EFCore.Sqlite;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCoreSqlite;

/// <summary>
/// Verifies that <see cref="SqliteRetryingExecutionStrategy"/> turns transient SQLite lock errors
/// (BUSY / LOCKED) into successful operations via retry.
/// </summary>
/// <remarks>
/// This is potentially the scenario behind issue #22939 where OpenIddict token reads via
/// EF Core failed while a long-running migration held a write lock. We parameterise across both
/// read and write operations because the reported customer stack trace was a read
/// (<c>SingleOrDefaultAsync</c> on the token table); the write path is included for symmetry.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class SqliteRetryingExecutionStrategyTests : UmbracoIntegrationTest
{
    // Keep the per-command SQLite busy-poll short so the baseline test fails quickly
    // and the resilient test exercises the EF Core retry loop (not SQLite's internal one).
    private const int CommandTimeoutSeconds = 1;

    // Hold the blocker write lock for longer than CommandTimeoutSeconds so a non-retrying caller
    // sees a SqliteException, but well within the retry strategy's budget so a retrying caller wins.
    private static readonly TimeSpan _blockerHoldDuration = TimeSpan.FromMilliseconds(2500);

    public enum Operation
    {
        // NUnit's [TestCase] attribute reflects over public enum values, so this stays public.
        Read,
        Write,
    }

    private IDbContextFactory<BaselineDbContext> BaselineFactory =>
        GetRequiredService<IDbContextFactory<BaselineDbContext>>();

    private IDbContextFactory<ResilientDbContext> ResilientFactory =>
        GetRequiredService<IDbContextFactory<ResilientDbContext>>();

    private string ConnectionString =>
        GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ConnectionString!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Register both DbContexts unconditionally. The lambda body only runs when a context is resolved,
        // and the tests below Ignore on non-SQLite — so a SQL Server run never executes UseSqlite.
        builder.Services.AddUmbracoDbContext<BaselineDbContext>(
            (IServiceProvider sp, DbContextOptionsBuilder options, string? connectionString, string? providerName) =>
            {
                options.UseSqlite(ApplyShortCommandTimeout(connectionString!));
            },
            shareUmbracoConnection: false);

        builder.Services.AddUmbracoDbContext<ResilientDbContext>(
            (IServiceProvider sp, DbContextOptionsBuilder options, string? connectionString, string? providerName) =>
            {
                options.UseSqlite(
                    ApplyShortCommandTimeout(connectionString!),
                    sqlite => sqlite.ExecutionStrategy(deps => new SqliteRetryingExecutionStrategy(deps)));
            },
            shareUmbracoConnection: false);
    }

    [TestCase(Operation.Read)]
    [TestCase(Operation.Write)]
    public async Task Cannot_Read_Or_Write_Via_EFCore_When_SQLite_Lock_Held_Beyond_Command_Timeout(Operation operation)
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            Assert.Ignore("SQLite-only scenario.");
        }

        using var blockerRelease = HoldBlockerLock();

        await using BaselineDbContext context = await BaselineFactory.CreateDbContextAsync();

        // SQLite's per-command busy-poll only waits CommandTimeoutSeconds; the blocker is held longer,
        // so without the retry strategy the operation must surface a SqliteException to the caller.
        // Both reads and writes route through EF Core's IExecutionStrategy when expressed via DbSet
        // (which is how OpenIddict's token store accesses umbracoOpenIddictTokens in the real failure).
        SqliteException? ex = Assert.ThrowsAsync<SqliteException>(
            async () => await ExecuteAsync(context.UmbracoLocks, operation));

        Assert.That(
            ex!.SqliteErrorCode,
            Is.AnyOf(raw.SQLITE_BUSY, raw.SQLITE_LOCKED),
            "Expected the failure to be a transient SQLite lock error — if this is something else, the test no longer reproduces issue #22939.");
    }

    [TestCase(Operation.Read)]
    [TestCase(Operation.Write)]
    public async Task Can_Read_And_Write_Via_EFCore_When_SQLite_Lock_Released_Within_Retry_Budget(Operation operation)
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            Assert.Ignore("SQLite-only scenario.");
        }

        using var blockerRelease = HoldBlockerLock();

        await using ResilientDbContext context = await ResilientFactory.CreateDbContextAsync();

        // With SqliteRetryingExecutionStrategy in place, the first attempt fails with BUSY/LOCKED
        // (same as the baseline), the strategy backs off and retries, and once the blocker releases
        // the operation succeeds — no exception bubbles out.
        Assert.DoesNotThrowAsync(async () => await ExecuteAsync(context.UmbracoLocks, operation));
    }

    /// <summary>
    /// Runs a single EF Core operation that routes through the configured
    /// <see cref="IExecutionStrategy"/>: either a <c>SingleOrDefaultAsync</c> read
    /// (matching OpenIddict's <c>FindByReferenceIdAsync</c> shape) or an
    /// <c>ExecuteUpdateAsync</c> write.
    /// </summary>
    private static async Task ExecuteAsync(DbSet<LockEntity> set, Operation operation)
    {
        switch (operation)
        {
            case Operation.Read:
                _ = await set.Where(x => x.Id == 1).SingleOrDefaultAsync();
                break;
            case Operation.Write:
                await set
                    .Where(x => x.Id == 1)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Value, x => x.Value));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }
    }

    /// <summary>
    /// Opens a separate SQLite connection on the same shared in-memory database,
    /// acquires a RESERVED write lock via a transactional UPDATE, and schedules the lock's
    /// release after <see cref="_blockerHoldDuration"/>. The returned <see cref="IDisposable"/>
    /// joins the release task on dispose so the test method always waits for cleanup.
    /// </summary>
    private BlockerLockHandle HoldBlockerLock()
    {
        SqliteConnection blocker = new(ConnectionString);
        blocker.Open();

        SqliteTransaction blockerTx = blocker.BeginTransaction();

        using (SqliteCommand cmd = blocker.CreateCommand())
        {
            cmd.Transaction = blockerTx;
            cmd.CommandText = "UPDATE umbracoLock SET value = value WHERE id = 1";
            cmd.ExecuteNonQuery();
        }

        Task release = Task.Run(async () =>
        {
            await Task.Delay(_blockerHoldDuration);
            blockerTx.Commit();
            blockerTx.Dispose();
            blocker.Dispose();
        });

        return new BlockerLockHandle(release);
    }

    private static string ApplyShortCommandTimeout(string connectionString)
    {
        // Microsoft.Data.Sqlite uses Default Timeout (seconds) as the SqliteCommand.CommandTimeout
        // default, which drives the internal busy-poll loop. Shortening it makes the baseline test
        // surface a SqliteException quickly instead of waiting the 30-second default.
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            DefaultTimeout = CommandTimeoutSeconds,
        };
        return builder.ConnectionString;
    }

    private sealed class BlockerLockHandle : IDisposable
    {
        private readonly Task _release;

        public BlockerLockHandle(Task release) => _release = release;

        public void Dispose() => _release.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Minimal entity mapped to the existing <c>umbracoLock</c> table so we can exercise EF Core's
    /// query / ExecuteUpdate pipeline (which uses the configured execution strategy).
    /// </summary>
    private class LockEntity
    {
        public int Id { get; set; }

        public int Value { get; set; }

        public string Name { get; set; } = null!;
    }

    private class BaselineDbContext : DbContext
    {
        public BaselineDbContext(DbContextOptions<BaselineDbContext> options)
            : base(options)
        {
        }

        public DbSet<LockEntity> UmbracoLocks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => MapLockEntity(modelBuilder);
    }

    private class ResilientDbContext : DbContext
    {
        public ResilientDbContext(DbContextOptions<ResilientDbContext> options)
            : base(options)
        {
        }

        public DbSet<LockEntity> UmbracoLocks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => MapLockEntity(modelBuilder);
    }

    private static void MapLockEntity(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LockEntity>(entity =>
        {
            entity.ToTable("umbracoLock");
            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(64).HasColumnName("name");
            entity.Property(e => e.Value).HasColumnName("value");
        });
}
