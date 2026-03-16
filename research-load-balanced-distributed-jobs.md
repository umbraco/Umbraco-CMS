# Research: IDistributedBackgroundJob Write Lock Timeout in Load-Balanced Setup

**Issue**: [#22113](https://github.com/umbraco/Umbraco-CMS/issues/22113)
**Error**: `Failed to acquire write lock for id: -347`
**Lock -347**: `Constants.Locks.DistributedJobs` (all distributed background jobs)

---

## Summary

The root cause is most likely **SQL Server page-level lock contention** on the `umbracoLock` table, caused by long-running content operations (inside the user's distributed job) holding REPEATABLEREAD locks on one row (e.g., `-333` ContentTree) which block write access to *all other rows on the same data page* (including `-347` DistributedJobs).

This is exacerbated by:
1. **Nested scope transaction sharing** - the user's outer scope holds the transaction (and all locks) open for the entire job duration
2. **Small table, single page** - all ~18 lock rows fit on one 8KB SQL Server data page
3. **5-second write lock timeout** - the default is too short when contention exists
4. **Backoffice activity** adding further lock pressure on the same table

---

## Detailed Analysis

### The Lock Table Problem

The `umbracoLock` table has approximately 18 rows (IDs -331 through -348). In SQL Server, a standard data page is 8KB. These 18 small rows (each just `id INT`, `name NVARCHAR`, `value INT`) **all fit on a single data page**.

SQL Server's lock granularity decisions:
- For small tables, the query optimizer may choose **page-level locks** instead of row-level locks
- The `WITH (REPEATABLEREAD)` table hint in the locking SQL means locks are held until the **end of the transaction**
- Without an explicit `ROWLOCK` hint, SQL Server decides the granularity

**Read lock SQL** (from `SqlServerDistributedLockingMechanism.cs:147`):
```sql
SELECT value FROM umbracoLock WITH (REPEATABLEREAD) WHERE id=@id
```

**Write lock SQL** (from `SqlServerDistributedLockingMechanism.cs:182-183`):
```sql
UPDATE umbracoLock WITH (REPEATABLEREAD) SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id
```

Neither uses a `ROWLOCK` hint, so SQL Server is free to use page-level locking.

### The Reproduction Scenario

Here's the exact sequence that causes the error:

**Server A** (running the user's distributed job):

1. `DistributedBackgroundJobHostedService` calls `TryTakeRunnableAsync()`
2. `TryTakeRunnableAsync` acquires `EagerWriteLock(-347)`, marks the "Clean Up Your Room" job as running, commits scope, **releases lock -347** -- this is fine
3. The user's `ExecuteAsync()` runs:
   ```csharp
   using ICoreScope scope = _scopeProvider.CreateCoreScope();  // ROOT scope, starts transaction

   _contentService.CountChildren(...)     // Creates NESTED scope, acquires ReadLock(-333)
   _contentService.RecycleBinSmells()     // Creates NESTED scope, acquires ReadLock(-333)
   _contentService.EmptyRecycleBin(...)   // Creates NESTED scope, acquires WriteLock(-333)

   scope.Complete();  // Transaction commits HERE, all locks released HERE
   ```

4. **Critical**: All nested scopes share the root scope's database/transaction (confirmed in `Scope.cs:350-360`). The `ReadLock(-333)` acquired by `CountChildren` is held until the ROOT scope disposes. If `EmptyRecycleBin` takes 30+ seconds (many items), the locks on row -333 are held for 30+ seconds.

5. With page-level locking, the shared (S) lock on row -333's **page** also covers row -347. This S lock blocks any exclusive (X) lock requests on the same page.

**Server B** (polling for jobs every 5 seconds):

6. `TryTakeRunnableAsync()` tries `EagerWriteLock(-347)`:
   ```sql
   SET LOCK_TIMEOUT 5000;
   UPDATE umbracoLock WITH (REPEATABLEREAD) SET value = ... WHERE id=-347
   ```
7. This UPDATE needs an exclusive (X) lock on row -347. But the page containing -347 has a shared (S) lock held by Server A's long-running transaction.
8. Server B **blocks for 5 seconds**, then gets SQL error 1222 (lock timeout)
9. This becomes: `DistributedWriteLockTimeoutException` → **"Failed to acquire write lock for id: -347"**

### Why Backoffice Login Triggers It

When users log into the backoffice and interact with content:

- **Listing content**: `ContentService.GetById/GetChildren` → `ReadLock(-333)`
- **Saving content**: `ContentService.Save` → `WriteLock(-333)`
- **Deleting content**: `ContentService.Delete/MoveToRecycleBin` → `WriteLock(-333)`
- **Publishing**: `ContentService.Publish` → `WriteLock(-333)`

Each of these acquires locks on the `umbracoLock` table. In load-balanced setups, backoffice web requests on *any server* add page-level lock contention on the same data page as -347. The more backoffice activity, the higher the probability that some transaction is holding a page lock that blocks -347 acquisition.

### Why It "Disables the Server Until Restart"

The `DistributedBackgroundJobHostedService` catches exceptions and continues (line 80). However:

1. Every 5 seconds, `TryTakeRunnableAsync` fails with the lock timeout
2. The error is logged each time, creating a flood of error logs
3. **No distributed jobs run on the affected server** because `TryTakeRunnableAsync` always times out
4. The user's custom job that's causing the contention (on the other server) eventually finishes, but by then the pattern of contention from backoffice operations may sustain the problem
5. The server appears "disabled" because its distributed job processing is effectively blocked

The server doesn't truly need a restart to recover, but the sustained contention from backoffice operations can make it *appear* permanently broken. A restart clears all in-flight transactions and ambient scopes, resolving the immediate contention.

---

## Contributing Factors

### 1. No `ROWLOCK` Hint

The distributed locking SQL uses `WITH (REPEATABLEREAD)` but not `WITH (ROWLOCK, REPEATABLEREAD)`. Adding `ROWLOCK` would force SQL Server to use row-level locks, preventing cross-row contention on the same page.

**File**: `src/Umbraco.Cms.Persistence.SqlServer/Services/SqlServerDistributedLockingMechanism.cs`
- Line 147 (read lock): `SELECT value FROM umbracoLock WITH (REPEATABLEREAD) WHERE id=@id`
- Line 182-183 (write lock): `UPDATE umbracoLock WITH (REPEATABLEREAD) SET value = ... WHERE id=@id`

### 2. Short Default Write Lock Timeout

**File**: `src/Umbraco.Core/Configuration/Models/GlobalSettings.cs`

The default write lock timeout is **5 seconds** (`DistributedLockingWriteLockDefaultTimeout`). In a load-balanced setup with active backoffice use, this is easily exceeded during page-level lock contention.

### 3. User's Outer Scope Prolongs Lock Duration

The user's code wraps multiple ContentService calls in a single scope:

```csharp
using ICoreScope scope = _scopeProvider.CreateCoreScope();
_contentService.CountChildren(...);      // ReadLock(-333) acquired, held by root transaction
_contentService.RecycleBinSmells();      // ReadLock(-333)
_contentService.EmptyRecycleBin(...);    // WriteLock(-333), potentially slow
scope.Complete();                        // ALL locks released here
```

The nested scopes created by ContentService methods all share the root scope's transaction (`Scope.cs:350-360`). This means the ReadLock from `CountChildren` is held for the entire duration of `EmptyRecycleBin`.

### 4. `Task.Run` in User Code

The user wraps their code in `Task.Run()`:
```csharp
public Task ExecuteAsync()
{
    return Task.Run(() => { ... });
}
```

While this doesn't directly cause the lock issue, `Task.Run` moves execution to a thread pool thread. This is unnecessary (the hosted service already runs on a background thread) and could cause issues with scope ambient context if the async context doesn't flow properly.

---

## Potential Fixes

### Fix 1: Add `ROWLOCK` Hint (Framework Fix - Recommended)

Add `ROWLOCK` to the SQL statements in `SqlServerDistributedLockingMechanism`:

```sql
-- Read lock
SELECT value FROM umbracoLock WITH (ROWLOCK, REPEATABLEREAD) WHERE id=@id

-- Write lock
UPDATE umbracoLock WITH (ROWLOCK, REPEATABLEREAD) SET value = ... WHERE id=@id
```

This forces SQL Server to use row-level locks, preventing cross-row contention within the same page. Row-level locks on id=-333 would NOT block row-level locks on id=-347.

**Impact**: Minimal. Row-level locks are slightly more expensive in memory (lock manager overhead) but the umbracoLock table is tiny. This is the standard best practice for small lookup tables where row independence is required.

The same fix should also be applied to the EF Core SQL Server locking mechanism:
- `src/Umbraco.Cms.Persistence.EFCore/Locking/SqlServerEFCoreDistributedLockingMechanism.cs`

### Fix 2: Separate Lock Tables (Framework Fix - More Invasive)

Move distributed job locks to a separate table (`umbracoDistributedJobLock`) so they can never share a page with content tree locks. This is more invasive but eliminates the problem entirely regardless of SQL Server lock granularity decisions.

### Fix 3: Increase Write Lock Timeout (User Workaround)

```json
{
  "Umbraco": {
    "CMS": {
      "Global": {
        "DistributedLockingWriteLockDefaultTimeout": "00:00:30"
      }
    }
  }
}
```

Increasing to 30 seconds gives more time for the contending transaction to complete. This is a workaround, not a fix - it trades timeout frequency for longer blocking delays.

### Fix 4: User Code Improvement (User Workaround)

The user should avoid wrapping multiple ContentService calls in a single outer scope. Each ContentService method already manages its own scope:

```csharp
public Task ExecuteAsync()
{
    // NO outer scope needed - each ContentService method creates its own scope
    int numberOfThingsInBin = _contentService.CountChildren(Constants.System.RecycleBinContent);
    _logger.LogInformation("You have {Count} items to clean", numberOfThingsInBin);

    if (_contentService.RecycleBinSmells())
    {
        _contentService.EmptyRecycleBin(userId: -1);
    }

    return Task.CompletedTask;
}
```

This reduces lock hold duration because each ContentService call acquires and releases its locks independently. The `CountChildren` ReadLock(-333) is released before `EmptyRecycleBin` starts.

Also: remove the `Task.Run` wrapper - it's unnecessary since the hosted service already runs on a background thread.

---

## Key Code References

| File | Purpose |
|------|---------|
| `src/Umbraco.Infrastructure/BackgroundJobs/DistributedBackgroundJobHostedService.cs` | Timer loop, calls TryTake → Execute → Finish |
| `src/Umbraco.Infrastructure/Services/Implement/DistributedJobService.cs` | Acquires WriteLock(-347) in TryTakeRunnableAsync (line 68) and FinishAsync (line 105) |
| `src/Umbraco.Cms.Persistence.SqlServer/Services/SqlServerDistributedLockingMechanism.cs` | SQL Server lock SQL (lines 147, 182-183) - missing ROWLOCK hint |
| `src/Umbraco.Core/Persistence/Constants-Locks.cs` | Lock ID definitions (-331 through -348) |
| `src/Umbraco.Infrastructure/Scoping/Scope.cs:350-360` | Nested scopes share parent's Database/transaction |
| `src/Umbraco.Core/Services/ContentService.cs` | EmptyRecycleBin acquires WriteLock(-333), CountChildren/RecycleBinSmells acquire ReadLock(-333) |
| `src/Umbraco.Core/Configuration/Models/GlobalSettings.cs` | Default lock timeout: 5 seconds for writes |

---

## Verification Steps

To confirm this hypothesis:

1. **SQL Server Activity Monitor**: During reproduction, check for page-level locks on the `umbracoLock` table using `sys.dm_tran_locks`:
   ```sql
   SELECT * FROM sys.dm_tran_locks
   WHERE resource_database_id = DB_ID()
   AND resource_associated_entity_id = OBJECT_ID('umbracoLock')
   ORDER BY request_mode, resource_type
   ```

2. **Check lock granularity**: Look for `resource_type = 'PAGE'` entries, which would confirm page-level locking.

3. **Test with ROWLOCK**: Temporarily modify the SQL to include `ROWLOCK` hint and verify the issue disappears.

4. **Test without outer scope**: Have the user remove the wrapping `CreateCoreScope()` call and verify the issue is mitigated (shorter individual lock durations).
