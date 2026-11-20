---
name: integration-branch-regressions-after-0911-mergeup
description: "Two regressions on v18/feature/ef-core-repositories from the 09-11-2026 v19/dev merge-up (c6b0ec18aa5), found while triaging PR #24542; both fixed 11-11-2026 (b62ac4c5bb4, 6d60c90c370), the cache-sharing hazard remains upstream"
metadata:
  node_type: memory
  type: project
  originSessionId: 83ff3bb1-6ad5-4778-b26e-0a7c2f58c918
  modified: 2026-11-11T09:41:17.070Z
---

Triage of PR #24542 (element campaign PR 4) on 11-11-2026 showed every pipeline failure is merge-up fallout from `c6b0ec18aa5`, not the PR. Both reproduce on that base commit and pass on the pre-merge commit `f53d471a4dd`. Fixed on the integration branch on 11-11-2026 as `b62ac4c5bb4` (webhook) and `6d60c90c370` (content types); the upstream `GetCacheItemAsync` sharing hazard is still live for any future EF Core repository that routes a database factory through it.

1. **WebhookRepository.DeleteAsync parameterises the table name.** The merge ported upstream #24441 as `db.Database.ExecuteSqlAsync($"DELETE FROM {Constants.DatabaseSchema.Tables.WebhookRequest} WHERE webhookKey = {webhookKey}")`. `ExecuteSqlAsync(FormattableString)` turns every hole into a parameter, so the SQL is `DELETE FROM @p0 ...` on every provider. Fails the six `WebhookServiceTests.Can_Delete*` cases (integration part 3). Fix: inline the table name (it is a constant), keep only the key parameterised, or map `WebhookRequestDto` into the EF Core model and use `ExecuteDeleteAsync`.
2. **Upstream in-flight cache de-duplication deadlocks EF Core content-type reads under concurrency.** Upstream #24302 (`47bfc992664`) made `AppCacheExtensions.GetCacheItemAsync` share one factory task across concurrent callers in a process-wide dictionary. On this branch only, `ContentTypeCommonRepository.GetAllTypesAsync` goes through it (upstream NPoco uses the sync `GetCacheItem`), so the factory runs on the first caller's scope/transaction. With the test harness's `NoCache` every call misses: a reader blocked on a writer's `umbracoNode` lock gets awaited by that writer inside its save transaction → 30 s command timeouts, then `database table is locked`. Fails `ThreadSafetyServiceTest.Ensure_All_Threads_Execute_Successfully_Content_Service` (always) and `_Media_Service` (Linux). Fix on this branch: stop routing that repository through `GetCacheItemAsync` (sync `GetCacheItem` or inline get/fetch/insert). Upstream: the cross-scope sharing is unsafe for any factory that touches the database through the ambient scope; raise it.

Also seen: E2E part 1 `BackofficeSearchDocument` "can find a document by name" timed out three times on the same run; passes on both merge parents; cause unknown (possibly regression 2 via `IContentTypeService.GetAllAsync` in the search item controller).

**Why:** Both block the campaign because they fail on the integration branch itself, and both will be mis-attributed to whichever campaign PR runs the pipeline next.

**How to apply:** Fix them on the integration branch (or a small PR into it), not inside a campaign PR. Related: [[ci-pipeline-never-excludes-longrunning-on-linux]], [[ef-core-element-repository-campaign]], [[merge-up-sync-async-twins-need-manual-port]].
