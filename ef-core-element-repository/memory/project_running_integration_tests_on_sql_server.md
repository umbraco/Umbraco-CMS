---
name: running-integration-tests-on-sql-server
description: "How to run Umbraco integration tests against SQL Server on each machine - env vars not appsettings, the Linux box uses a docker container on port 14333, verify with the detailed-verbosity banner, and never let a run go without a timeout."
metadata:
  node_type: memory
  type: project
  originSessionId: 4f8da05c-fe36-4b55-8eed-9028f7cca30c
  modified: 2026-11-10T11:43:02.740Z
---

Run integration tests against SQL Server with **environment variables**, never by editing
`tests/Umbraco.Tests.Integration/appsettings.Tests.json`:

```bash
Tests__Database__DatabaseType="SqlServer" \
Tests__Database__SQLServerMasterConnectionString="<master connection string>" \
timeout 1200 dotnet test tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --no-build -c Debug \
  --filter "<targeted filter>" --logger "console;verbosity=detailed"
```

`GlobalSetupTeardown` builds config as json → `appsettings.Tests.Local.json` (optional) → env vars, so
env vars win.

**Which server exists depends on the machine.**
- The Linux box (`/home/mole/github/V18/Umbraco-CMS`, checked 10-11-2026): nothing listens on 1433 and
  there is no `mssql-server` service. SQL Server is the stopped docker container `sql-9200fb85`
  (`mcr.microsoft.com/mssql/server:2022-latest`) mapped to `127.0.0.1:14333`. `docker start sql-9200fb85`,
  probe with `docker exec sql-9200fb85 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PW" -C -Q "SELECT 1"`,
  read the SA password from the container env (`docker inspect`, `MSSQL_SA_PASSWORD`) into a shell
  variable without printing it, connection string `Server=127.0.0.1,14333;User Id=sa;Password=...;TrustServerCertificate=true;`.
  Stop the container again afterwards (it was stopped when found).
- The Windows box: SQL Server 2019 default instance with Windows auth,
  `Server=localhost;Integrated Security=true;TrustServerCertificate=true;`.

**Always wrap the run in `timeout`.** With no server reachable the run does not fail fast: on 09-11-2026
it sat at 0% CPU for 25 hours before being killed. The user said to skip local SQL Server runs for PR 4
and let the pipeline cover them; offer the pipeline before spending time on a local server.

**Verify where it actually ran.** The only sound check is the banner, which needs `verbosity=detailed`:

```
* DatabaseType     : SqlServer
```

Two traps that both produced wrong conclusions once:
- At `verbosity=minimal` the banner is suppressed; a run reported 4/4 passed while silently on SQLite.
- Checking the server for leftover `UmbracoTests-*` databases proves nothing — `GlobalSetupTeardown.TearDown`
  drops every one of them on completion.

**Do NOT use `DatabaseType=LocalDb`.** That mode attaches auto-named `.mdf` files and wedged for 5 hours
against a stale file. The `SqlServer` mode creates `UmbracoTests-1..N` through the master connection
string instead and is fast — 395 tests in about 6 minutes on the Windows box.

**Result 2026-09-23 (Windows):** `DocumentRepositoryTest` + `DocumentBlueprintRepositoryTest` + `ContentServiceTests`
= 393/395 on SQL Server. The 2 failures were pre-existing raw test SQL using the T-SQL reserved word
`current` unquoted (fixed in `b8ce78e074e` by bracket-quoting). No EF Core document-path failures.

Related: [[efcore-handover-premises-unreliable]], [[filter-by-touched-test-not-touched-file]], [[only-new-tests-locally]].
