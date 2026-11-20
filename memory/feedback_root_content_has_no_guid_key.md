---
name: root-content-has-no-guid-key
description: "Constants.System.RootKey is null — root-level content (int id -1) has no Guid identity, so resolving int parentId -> Guid via IIdKeyMap silently fails for root and breaks Guid-only async repository methods"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 3629769a-7eca-4d89-9fd7-e4c33337b0f8
  modified: 2026-08-18T08:17:46.978Z
---

`Constants.System.Root` (int, `-1`) represents the content tree root, but `Constants.System.RootKey`
(`src/Umbraco.Core/Constants-System.cs:26`) is literally `public static readonly Guid? RootKey = null;`.
Root has no Guid identity at all — it isn't a real `umbracoNode` row, so `IIdKeyMap.GetKeyForIdAsync(-1, ...)`
fails (no special-casing for it in `IdKeyMap.GetKeyForIdAsync`, only the recycle-bin ids are special-cased),
and the new Guid-only async repository methods (`GetChildrenAsync(Guid parentKey, ...)`,
`GetDescendantsAsync`, etc.) have no way to represent "children of the literal root" — `ResolveNodeIdAsync`
just returns `0` (int default) for an unresolvable key, not `-1`.

**Why:** Migrating `ContentEditingService.GetPagedChildrenAsync`'s override to the new
`IContentService.GetChildrenAsync(Guid, ...)` broke root-level content sorting silently — no exception,
just an empty page returned for any `parentId == Constants.System.Root`, making the whole Sort operation
a silent no-op. Caught by `ContentEditingServiceTests.SortByField.cs`'s root-content test cases (7 failures);
one test case coincidentally still passed because "sort did nothing" happened to match that one expected
order (creation order == expected order for CreateDate-ascending). This is exactly the kind of
silently-wrong-not-crashing bug this codebase's testing discipline exists to catch — don't assume "some
items came back" means resolution succeeded; check whether it's actually querying what you think it is.

**How to apply:** Whenever migrating an `int`-keyed method that can legitimately receive
`Constants.System.Root`/`-1` as a parent/target onto a new Guid-keyed async repository method, don't
assume "id → key → new async method" is a safe blanket transformation — check whether `-1` (or the
recycle bin ids, which — unlike root — *are* special-cased in `IIdKeyMap`) can actually reach that call
site first.

**Preferred fix (confirmed better than a fallback)**: widen the new method's parameter to `Guid?` and
treat `null` as "root of the content tree" — this matches an established codebase convention
(`ContentCreateModel.ParentKey`, `Constants.System.RootKey` itself) where `null` already means root
elsewhere. In the repository implementation, skip `ResolveNodeIdAsync` entirely when the key is `null`
and use `Constants.System.Root` directly as the node id. This is a source-compatible widening for every
existing caller passing a real Guid (implicit `Guid` → `Guid?` conversion), so check first whether the
method has a single concrete implementer (cheap to verify via grep) before assuming the change is safe —
it was, here, since `AsyncDocumentRepository` was the only implementer of `GetChildrenAsync`. Prefer this
over keeping a sync/int fallback for the root case alone — a fallback still leaves two code paths to
maintain, while widening to `Guid?` unifies everything under the one async method.
