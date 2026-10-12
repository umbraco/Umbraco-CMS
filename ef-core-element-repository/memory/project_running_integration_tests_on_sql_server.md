---
name: running-integration-tests-on-sql-server
description: "How to run Umbraco integration tests against the local SQL Server instance - use env vars not appsettings, and verify with the detailed-verbosity banner."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-23T11:44:46.588Z
---

This machine has **SQL Server 2019** running as the default instance (`MSSQLSERVER`), reachable with
Windows auth; the account is sysadmin, so it can create databases.

Run integration tests against it with **environment variables**, never by editing
`tests/Umbraco.Tests.Integration/appsettings.Tests.json`:

```bash
Tests__Database__DatabaseType="SqlServer" \
Tests__Database__SQLServerMasterConnectionString="Server=localhost;Integrated Security=true;TrustServerCertificate=true;" \
dotnet test tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --no-build -c Debug \
  --filter "<targeted filter>" --logger "console;verbosity=detailed"
```

`GlobalSetupTeardown` builds config as json → `appsettings.Tests.Local.json` (optional) → env vars, so
env vars win. Keeping the file untouched removes any chance of committing the switch, and Windows auth
keeps credentials out of the repo entirely.

**Verify where it actually ran.** The only sound check is the banner, which needs
`verbosity=detailed`:

```
* DatabaseType     : SqlServer
```

Two traps that both produced wrong conclusions once:
- At `verbosity=minimal` the banner is suppressed; a run reported 4/4 passed while silently on SQLite.
- Checking the server for leftover `UmbracoTests-*` databases proves nothing — `GlobalSetupTeardown.TearDown`
  drops every one of them on completion.

**Do NOT use `DatabaseType=LocalDb`.** That mode attaches auto-named `.mdf` files and wedged for 5 hours
against a stale file (0% CPU, no DB sessions, prepared-database pool never refilled). The `SqlServer`
mode creates `UmbracoTests-1..N` through the master connection string instead and is fast — 395 tests in
about 6 minutes.

**Result 2026-09-23:** `DocumentRepositoryTest` + `DocumentBlueprintRepositoryTest` + `ContentServiceTests`
= 393/395 on SQL Server. The 2 failures were pre-existing raw test SQL using the T-SQL reserved word
`current` unquoted (fixed in `b8ce78e074e` by bracket-quoting). No EF Core document-path failures.

Related: [[efcore-handover-premises-unreliable]], [[filter-by-touched-test-not-touched-file]].
