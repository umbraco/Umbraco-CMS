---
name: asyncentityrepositorybase-cancellationtoken-not-forwarded
description: "Unfixed: AsyncEntityRepositoryBase.GetAsync accepts a CancellationToken but never forwards it. NOT a narrow fix - attempted 2026-09-21 and reverted; it cannot be scoped to one entity type because the Perform* methods are abstract on the shared base, so all 13 async repositories change at once (160 compile errors immediately)."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-21T09:06:06.334Z
---

`AsyncEntityRepositoryBase<TKey, TEntity>` accepts a `CancellationToken` on `GetAsync`, `GetManyAsync`,
`GetAllAsync`, `ExistsAsync` and `DeleteAsync` and forwards it nowhere. The token is inert past the
service tier for every entity type inheriting this base.

## Attempted 2026-09-21 and REVERTED — read this before trying again

The previous version of this note called it "a dedicated, narrow fix… low risk… purely internal
plumbing". **That is wrong and it caused a wasted attempt.** What actually happens:

**It cannot be scoped to the content/document path.** `PerformGetAsync`, `PerformGetAllAsync`,
`PerformGetManyAsync` are **abstract** on the base and `PerformExistsAsync` is virtual. Adding a
parameter to an abstract method breaks every override, so touching the base produced **160 compile
errors across 13 repositories instantly** — audit, dictionary, domain, key-value, language,
public-access, redirect-URL, relation, relation-type, content-type, document. There is no piecemeal
version. Either it is a deliberate cross-cutting change with its own plan and review, or it is not done.

**A signature-only change makes things worse, not better.** After adding the parameter everywhere,
**41 of 45 `Perform*` bodies ignored it** — i.e. 41 methods advertising cancellation they do not honour.
The lie just moves one level down. Doing it honestly means editing the bodies too.

## What the attempt did establish (useful next time)

- **The cache policy interface does NOT need to change.** `IAsyncRepositoryCachePolicy` takes
  **delegates** (`Func<TKey?, Task<TEntity?>> performGet`), so the token can be captured in a closure at
  the call site: `CachePolicy.GetAsync(key, k => PerformGetAsync(k, ct), () => PerformGetAllAsync(ct))`.
  This avoids editing the 7 policy types — which matters, because those are the same files the team is
  looking at for the separately-reported cache-key bug
  (see [[async-repo-historical-version-property-data]] sibling report in the scratchpad).
- **Remainder if you take the delegate-capture route**: the policy's own I/O stays un-cancellable —
  `EnsureCacheIsSyncedAsync` and the count callback in `AsyncRepositoryCachePolicyOptions`.
- **Mechanical body fix works for most of them**: appending `, cancellationToken` to EF terminal
  operators (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, …) fixed ~32 of the 41.
- **The 9 that remain split three ways**: genuinely token-free (in-memory `GetAllCached()` in
  `AsyncContentTypeRepositoryBase`, and an `AuditRepository.PerformGetAllAsync` that only throws);
  delegating to an internal helper that also needs threading (`DocumentRepository.PerformGetRangeAsync`,
  `CommonRepository.GetAllTypesAsync`); and one that **actively discards** it —
  `DomainRepository.PerformGetAsync` passes `CancellationToken.None` to its own `GetAllAsync`.
- **Gotcha**: `AuditEntryRepository.PerformGetManyAsync(params int[] ids)` — `params` must be the last
  parameter, so the modifier has to go (the base does not declare it anyway).

## When to do it

Alongside the Element conversion, which touches this base regardless — that makes the cross-cutting
blast radius a cost already being paid rather than a surprise. See
[[document-repository-retirement-plan]].
