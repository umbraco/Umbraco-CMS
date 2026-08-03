using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// EF Core execution strategy that retries on transient SQLite errors (BUSY / LOCKED).
/// </summary>
/// <remarks>
/// <para>
/// SQLite serialises writers at the database level, and schema-modifying statements briefly
/// block readers — even in WAL mode. Without retries, concurrent EF Core reads (for example
/// OpenIddict's token validation against <c>umbracoOpenIddictTokens</c>) surface those
/// transient locks as <see cref="SqliteException"/> and fail the caller's request.
/// </para>
/// <para>
/// Microsoft does not ship a built-in execution strategy for SQLite (only the SQL Server
/// equivalent), so we provide this one. It piggy-backs on <see cref="ExecutionStrategy"/>'s
/// default exponential backoff and re-uses its inherited
/// <see cref="ExecutionStrategy.DefaultMaxRetryCount"/> (6) and
/// <see cref="ExecutionStrategy.DefaultMaxDelay"/> (30 seconds), which produce a delay
/// schedule of roughly 0s, 1s, 3s, 7s, 15s, 30s — a ~56-second retry window.
/// </para>
/// <para>
/// On top of those EF Core delays, <c>SQLITE_BUSY</c> (error 5) is also retried internally
/// by Microsoft.Data.Sqlite for up to the connection's <c>Default Timeout</c> (30 seconds
/// by default) per attempt. <c>SQLITE_LOCKED</c> (error 6) is not — it returns immediately,
/// so EF Core's retry budget is the only buffer.
/// </para>
/// </remarks>
public class SqliteRetryingExecutionStrategy : ExecutionStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteRetryingExecutionStrategy"/> class
    /// with default retry settings inherited from <see cref="ExecutionStrategy"/>.
    /// </summary>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    public SqliteRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : this(dependencies, DefaultMaxRetryCount, DefaultMaxDelay)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteRetryingExecutionStrategy"/> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="maxRetryDelay">The maximum delay between retries.</param>
    public SqliteRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay)
        : base(dependencies, maxRetryCount, maxRetryDelay)
    {
    }

    /// <inheritdoc />
    /// <remarks>
    /// When an explicit transaction is already open (Umbraco's <c>EFCoreScope</c> opens one for
    /// every unit of work) the operation is executed directly without retries. A retrying strategy
    /// cannot retry an operation that participates in a caller-managed transaction, and EF Core
    /// throws rather than allow it. Consumers without an ambient transaction — notably OpenIddict's
    /// token store — still get the retry behaviour from <see cref="ExecutionStrategy"/>.
    /// </remarks>
    public override TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        if (Dependencies.CurrentContext.Context.Database.CurrentTransaction is not null)
        {
            return operation(Dependencies.CurrentContext.Context, state);
        }

        return base.Execute(state, operation, verifySucceeded);
    }

    /// <inheritdoc />
    /// <remarks>
    /// See <see cref="Execute{TState,TResult}"/> for why an open transaction bypasses the retry.
    /// </remarks>
    public override Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
    {
        if (Dependencies.CurrentContext.Context.Database.CurrentTransaction is not null)
        {
            return operation(Dependencies.CurrentContext.Context, state, cancellationToken);
        }

        return base.ExecuteAsync(state, operation, verifySucceeded, cancellationToken);
    }

    /// <inheritdoc />
    protected override bool ShouldRetryOn(Exception exception)
    {
        // EF Core wraps provider exceptions, so walk the inner-exception chain.
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqlite && sqlite.IsBusyOrLocked())
            {
                return true;
            }
        }

        return false;
    }
}
