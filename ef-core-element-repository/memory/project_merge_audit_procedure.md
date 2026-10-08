---
name: project_merge_audit_procedure
description: Three mechanical checks that catch work lost in a merge-up between the EF Core branches — missing-added-lines, fork cross-reference, and normalised method-pair diff
metadata:
  type: project
---

After resolving conflicts on a merge-up, run all three — each catches a different class of loss:

1. **Missing-added-lines scan.** For every file changed by *both* sides (`comm -12` of the two
   `git diff --name-only <merge-base> ...` lists), check each line the other side added is present
   in the merged file. Anything reported is either a deliberate adaptation (async/EF Core form) or
   a lost change; there is no third category.
2. **Fork cross-reference.** For each `Async*.cs`, find the same name without the prefix **anywhere
   in the repo** (the EF Core forks sit in a different directory from their twin, so a same-directory
   search misses them) and check whether the other side changed it. See [[project_async_fork_merge_hazard]].
3. **Normalised method-pair diff.** Extract a forked method pair with
   `awk -v pat="    <signature>" 'index($0,pat)==1,/^    }$/'`, normalise the intended naming
   differences (`EFCoreDtos.` prefix, `BuildEFCoreDto`→`BuildDto`), then `diff`. What survives is
   real drift. This is the only check that finds a fork living inside a shared file.

Then `dotnet build` (the first `--no-incremental` pass fails MSB3030 on
`appsettings-schema.Umbraco.Cms.json`, which the JsonSchema tool generates during that same build —
not a real error), then unit + targeted integration tests.

**Why:** the compiler catches async-API breakages ([[project_v19_dev_merge_async_reconciliation]]);
nothing catches a fix that landed in a stale twin.

**How to apply:** `dotnet build` also rewrites three `package-lock.json` files with local-npm churn —
`git checkout --` them before committing, per [[feedback_watch_for_stray_file_changes]]. After any
scripted bulk edit, diff the whole file, not the match count ([[feedback_regex_bulk_edit_overmatch]]).

**Fourth check (added 2026-10-08): lost usings.** Git's auto-merge happily drops a `using` that upstream removed as unused while our async code still needs it (`TemplateService` lost `Configuration.Models`, the Search.Core strategies lost `Umbraco.Cms.Core`). The compiler catches these, so always finish with a full `dotnet build umbraco.sln`; with the .NET 11 SDK the first solution build can fail MSB3030 on `appsettings-schema.json` and simply needs a second run. Also run the missing-added-lines scan over `tests/` as well as `src/`; the 2026-10-08 merge produced only async-adaptation hits in both, which is the expected clean result.
