---
name: merge-up-sync-async-twins-need-manual-port
description: "On every v19/dev merge-up into the ef-core-repositories branch, changes git lands in PublishableContentServiceBase (sync, used by ElementService) must be hand-ported into AsyncPublishableContentServiceBase (used by ContentService); git has no idea they are twins"
metadata:
  node_type: memory
  type: project
  originSessionId: 8c44f8f9-e415-4371-b468-f74b090ffba4
  modified: 2026-10-12T09:13:21.952Z
---

The branch has two publish engines: `PublishableContentServiceBase` (sync, still live for `ElementService`) and `AsyncPublishableContentServiceBase` (async, live for `ContentService`). Upstream only knows the sync one, so a merge-up applies fixes to the sync file and the async twin silently keeps the old behaviour. Same for `ContentService` sync methods upstream touches that exist here only as `*Async` (conflicts surface those, the base twin does not).

Seen 2026-10-12 merging v19/dev commit 043b6858906 (invariant-for-variant user-group permission): the whole `includeInvariantForVariant` / `GetRequiredUser` plumbing had to be ported by hand into the async base, and `ImpactExplicit(culture, bool)` kept compiling with the old `isDefault` bool meaning the new `includeInvariantForVariant` parameter.

**Why:** regressions here compile cleanly; only a diff of the two bases or a behavioural test catches them.

**How to apply:** after any merge-up, `git diff <merge-base> MERGE_HEAD -- src/Umbraco.Core/Services/PublishableContentServiceBase.cs src/Umbraco.Core/Services/ContentService.cs` and walk every hunk into the async twin. Also: an upstream NPoco column add (e.g. `UserGroupDto`) needs the EF Core DTO + config + a no-op snapshot migration here (pattern: `RemoveUserGroupDefaultPermissionsFromModel`), otherwise every integration test fails with PendingModelChangesWarning. See [[efcore-handover-premises-unreliable]] and [[no-obsolete-constructors-on-this-branch]] (upstream obsolete forwarding ctors were dropped on merge to match the branch).

**09-11-2026 merge of v19/dev (`fb17c75d01a`, 300 commits, 36 conflicted files) — what had to be carried by hand, as a checklist for the next one:**
- Upstream `ContentSchedule` gained `IncludeInvariantForVariant` plus an NPoco premigration column on `umbracoContentSchedule`: the EF Core `ContentScheduleDto` + configuration (`HasDefaultValue(false)`), the mapping in `AsyncPublishableContentRepositoryBase` (read, update, insert), the scheduled-release flag in `AsyncPublishableContentServiceBase.PerformScheduledPublishingReleaseAsync`, and a no-op snapshot migration `AddIncludeInvariantForVariantToContentScheduleModel` (enum 26) were all missing until done by hand. `has-pending-model-changes` needs the global `dotnet-ef` on the package's EF version (updated to 11.0.0-rc.3).
- Upstream refactored container moves into `EntityTypeContainerService` with sync hooks `GetContainedEntity(int)`/`SaveContainedEntity`; our EF Core repositories are Guid-keyed and async, so the hooks became `GetContainedEntityAsync(IEntitySlim descendant)`/`SaveContainedEntityAsync` and every container service override was rewritten (blueprints use `descendant.Key`, element/data types wrap their sync repositories).
- Upstream moved sync `GetPagedOfTypes` onto `IPublishableContentService`; the async tier got `GetPagedOfTypesAsync(Guid[] keys, skip, take, ordering, ct)` moved off `IContentService`, with a bridge on the sync `PublishableContentServiceBase` (skip must be a multiple of take) until PR 5 replaces `ElementService`.
- `PropertyFactory.BuildEFCoreDtosAsync` needed upstream's `dataEditor` hoist + `ArePropertyValuesEquivalent`; `AsyncContentRepositoryBase.LoadPropertyDataAsync` needed the alias parameter-budget guard from `ContentRepositoryBase.GetPropertyCollections`.
- `[GeneratedRegex]` upstream makes `ContentTypeEditingServiceBase` `partial`; the `Async*` fork must be `partial` too (CS0260). Rename detection merged upstream's edits into the fork automatically this time.
- Sync-API usages in new upstream files (`NewDefaultUrlProvider`, `BatchElementsController`, `ElementIndexingNotificationHandler`, the element paths in `DraftContentChangeStrategy`/`ContentTypeIndexingService`) only show up as compile errors; the test projects then needed ~200 call sites ported (delegated to one agent with the API table; unit 7881 green, 588 touched integration tests green).
- Upstream's NPoco `WebhookRepository` now deletes queued requests first; the EF Core model has no request DTO, so the EF Core repository issues a raw `DELETE` for that table.
