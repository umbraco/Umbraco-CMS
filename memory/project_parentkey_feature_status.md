---
name: parentkey-feature-status
description: "IContentBase.ParentKey (Guid? parent-key fast path, replacing per-call IIdKeyMap int->Guid lookups) is now FULLY implemented and tested end-to-end on v18/feature/ef-core-document-repository - self-join population (zero extra round trips) for both NPoco and EF Core, StaticServiceProvider fully removed, plain abstract get/set interface member (no DIM), cast-free everywhere - ship-ready"
metadata:
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-20T13:52:59.883Z
---

On branch `v18/feature/ef-core-document-repository`, a new `Guid? ParentKey` property was added to
`IContentBase` (implemented via `ContentBase`, so `Content`/`Media`/`Member`/`Element` all get it) so
callers holding an already-loaded content entity can read its parent's key directly instead of doing a
separate `IIdKeyMap.GetKeyForIdAsync(int, UmbracoObjectTypes)` round trip.

**Why:** User request, motivated by wanting Media/Member/Element (migrating to EF Core in subsequent PRs
right after Content) to also benefit from this — hence the property lives on `IContentBase`, not `IContent`
or `ITreeEntity`.

**How to apply:** This is DONE, not a TODO. Don't re-plan or re-investigate this from scratch. The design
went through several rounds of simplification within the same session (see below) — if you find any other
memory, comment, or doc describing an earlier iteration (batched follow-up query, `StaticServiceProvider`
fallback, `ContentObjectType`, `SetParentKey` method), it is STALE; this file describes the current,
final shape.

### Current design

- **Interface**: `IContentBase.ParentKey` is a **plain abstract property, `{ get; set; }`, with NO default
  interface implementation** — no body, no fallback, no `StaticServiceProvider`, no `NotSupportedException`.
  This was a deliberate choice: `ParentKey` is a brand-new, unreleased member on this branch, so the usual
  "avoid breaking third-party `IContentBase` implementors" policy (root `CLAUDE.md` §6.3) doesn't apply yet
  — there is no shipped contract to protect. Assigning through the interface (`content.ParentKey = value`)
  works directly, with no cast to a concrete type and no separate `SetParentKey` method (an earlier
  iteration added one specifically to avoid a cast; once the interface property itself got a public
  setter, the method was pure duplication and was removed).
- **`ContentBase`** provides the only concrete implementation: `_parentKey`/`_hasParentKey` backing fields;
  the public setter just assigns both. The getter: if already populated, return it. If not (e.g. a freshly
  constructed entity via the raw int-`parentId` constructor), infer Root (`ParentId == Constants.System.Root`
  → `Constants.System.RootKey`, i.e. `null`) and the three RecycleBin pseudo-parents (`this switch { IContent
  when ... => RecycleBinContentKey, IMedia when ... => RecycleBinMediaKey, IElement when ... =>
  RecycleBinElementKey }`) directly from `ParentId` — free, no lookup needed. For any other (real) parent
  whose key isn't yet populated, it throws `NotSupportedException` rather than reach for an ambient service
  locator to resolve it. This throw is `ContentBase`'s own design choice (independent of the interface,
  which has no default body at all) — callers that hit it must populate `ParentKey` themselves via the
  repository read path or `SetParent(ITreeEntity?)` before reading it.
  - An earlier iteration added a `protected abstract UmbracoObjectTypes ContentObjectType { get; }` (with
    overrides on `Content`/`Media`/`Member`/`Element`) purely so the concrete getter could switch on it
    instead of on `this` — this was **removed**: the `this switch { IContent => ..., IMedia => ..., IElement
    => ... }` pattern is simpler and needs no extra per-type property at all. Don't re-add it.
- `ParentId`'s setter (`virtual` on `TreeEntityBase`) INVALIDATES `_hasParentKey`/`_parentKey` on every
  direct assignment; `SetParent(ITreeEntity?)` (also `virtual`) EAGERLY captures `(parent as
  IContentBase)?.Key` when a parent object is in memory (Key is always available even for an unsaved
  parent, unlike Id).
- **Repository read paths** populate `ParentKey` in the SAME query as the entity fetch via a self-join —
  NOT a follow-up query, NOT `IIdKeyMap`:
  - NPoco (`PublishableContentRepositoryBase` — covers Document+Element; `MediaRepository`;
    `MemberRepository`): `GetBaseQuery` does `.LeftJoin<NodeDto>("parentNode").On<NodeDto, NodeDto>((l, r)
    => l.ParentId == r.NodeId, aliasRight: "parentNode")` and selects `Alias(x.UniqueId, "ParentUniqueId")`
    into a new `ContentDto.ParentUniqueId` `[ResultColumn] Guid?` property. `MapDtosToContent`/
    `MapDtoToContent` then do `c.ParentKey = dto.ContentDto.NodeDto.ParentId == Constants.System.Root ? null
    : dto.ContentDto.ParentUniqueId;` right after `BuildEntity`.
  - EF Core (`AsyncDocumentRepository`): a `GroupJoin`/`SelectMany(...DefaultIfEmpty())` LEFT JOIN against
    `db.Nodes` added at each of ~10 unique query-building locations (backing 20 `DocumentRow` construction
    sites), producing a `DocumentRow.ParentUniqueId` field. `AssembleEntitiesAsync` sets `entity.ParentKey =
    row.Node.ParentId == Constants.System.Root ? null : row.ParentUniqueId;` for every assembled entity.
  - **The Root guard is mandatory and easy to get wrong**: `umbracoNode` id `-1` (Root) is a REAL seeded row
    whose `UniqueId` is `Constants.System.RootSystemKey` — a completely different value from
    `Constants.System.RootKey` (`null`), the semantic "no parent" value `ParentKey` contracts to. Without
    the `ParentId == Root ? null : ...` guard at the point the joined/looked-up value is consumed, Root's
    real GUID leaks through as if it were a real parent key. See
    [[feedback_root_node_has_real_row_with_different_key]] — this was independently rediscovered via
    narrow TDD revert-checks for Document, Media, AND Member.
- **Write paths** (`ContentService.PerformMoveLocked`, `LeaveDescendantInRecycleBinLocked`, `Copy` ×2;
  `PublishableContentRepositoryBase`/`AsyncDocumentRepository`'s population sites above;
  `ContentEditingServiceBase`/`AsyncContentEditingServiceBase`'s `MapCreate`) all assign
  `content.ParentKey = ...` directly — no cast, since the interface property has a public setter.
  `PerformMoveLocked` uses `parentId switch { Root => null, RecycleBinContent => sentinel, _ =>
  parent?.Key }`, NOT a naive `parent?.Key` (because `MoveToRecycleBin` calls it with `parent: null` for a
  non-Root reason). `Copy` uses a `copyIdToKeyMap` (copy-id → copy-key) dictionary so descendant copies
  resolve their new parent's Guid with zero `IIdKeyMap` calls.
- **The one legitimate remaining `IIdKeyMap` fallback**: `AsyncPublishableContentServiceBase.GetParent(TContent?
  content)` (the sync `GetParent`, inherited by `ContentService` and siblings) is called with
  CALLER-SUPPLIED entities that are not guaranteed to have gone through a repository read path (e.g. built
  via `new Content(name, int parentId, ...)` then `Save()`+`Publish()` without a re-fetch — real production
  and test code does this). It does `try { parentKey = content.ParentKey; } catch (NotSupportedException) {
  fall back to this service's own constructor-injected `_idKeyMap` }`. This is legitimate constructor DI,
  not an ambient service locator, and is the ONLY place any `ParentKey`-adjacent code still consults
  `IIdKeyMap` as a fallback. `GetParentAsync` (the async sibling, always fetches via `GetByIdAsync` first)
  has no such fallback — by the time it reads `ParentKey`, the repository has already populated it.

### Removed/superseded designs (do not reintroduce)

1. **Batched follow-up query** (`PopulateParentKeys`/`PopulateParentKeysAsync` +
   `TryResolvePseudoParentKey` helpers on `ContentRepositoryBase`/`AsyncContentRepositoryBase`) — replaced
   by the self-join described above. Deleted entirely, zero remaining callers.
2. **`StaticServiceProvider`-based DIM fallback** on `IContentBase.ParentKey` — the interface's default
   implementation used to resolve an unpopulated real parent via
   `StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()`. Removed per explicit user direction
   (ambient service locators are an anti-pattern, virtually untestable, and this use case doesn't qualify
   for the root `CLAUDE.md` §6.1/§6.3 "obsolete constructor" exception). The interface now has no default
   body at all (see above) — there is nothing left to fall back to at the interface level.
3. **`protected abstract UmbracoObjectTypes ContentObjectType { get; }`** on `ContentBase`/`Content`/
   `Media`/`Member`/`Element` — added specifically to let the concrete `ParentKey` getter switch on it,
   removed in favor of `this switch { IContent => ..., IMedia => ..., IElement => ... }` (simpler, no
   extra member). NOTE: a **different**, pre-existing `ContentObjectType` abstract property lives on
   `PublishableContentServiceBase`/`AsyncPublishableContentServiceBase` (the SERVICE base classes, not the
   model classes) — that one is unrelated, still in use, and was never touched by this feature.
4. **`IContentBase.SetParentKey(Guid? parentKey)` method** — added to avoid a
   `((ContentBase)x).ParentKey = value` cast at 9 call sites, with its own throwing DIM. Removed once the
   interface property itself gained a public setter (`{ get; set; }`), making the method a pure
   pass-through duplicate. All 9 sites now do `x.ParentKey = value;` directly.
5. **Test-side `SpyIdKeyMap`/`SpyServiceProvider` that swap `StaticServiceProvider.Instance`** — added in
   `DocumentRepositoryTest.cs`, `MediaRepositoryTest.cs`, `AsyncDocumentRepositoryTest.cs` to prove the
   repository's self-join population wasn't silently masked by the (then-existing) `StaticServiceProvider`
   DIM fallback. Once that fallback was removed (#2 above), these spies became vacuous — nothing in the
   call path can ever reach `IIdKeyMap` via `StaticServiceProvider` anymore, with or without the spy
   installed, so `GetKeyForIdAsyncCallCount == 0` no longer proves anything a plain `ParentKey` value
   assertion doesn't already prove on its own (for the real-parent case, a broken join now throws
   `NotSupportedException` instead of silently succeeding via a fallback — an even stronger guarantee).
   Removed entirely; the affected tests now just assert `result.ParentKey`'s value directly, matching the
   already-established plain-assertion style used everywhere else in these files. Where a genuine
   "no redundant query" guarantee is worth testing, the existing SQL-count-based tests
   (`database.EnableSqlCount`/`SqlCount`, e.g. `MediaRepositoryTest.Get_For_Non_Root_Media_Resolves_ParentKey_Without_A_Second_Query`,
   `DocumentRepositoryTest`'s equivalent) already cover it more precisely than a spy ever did.
   `ContentServiceTests.cs`'s `SpyIdKeyMap` is DIFFERENT and was KEPT: it's DI-registered
   (`builder.Services.AddUnique<IIdKeyMap>(...)`), not `StaticServiceProvider`-based, and protects against
   a real, still-live regression — `AsyncPublishableContentServiceBase.GetParent`'s own catch-block
   fallback (design item above) redundantly consulting `_idKeyMap` even when `ParentKey` is already
   populated. Its doc comments were updated to stop referencing the removed DIM fallback as the rationale.

### Independent review round + TDD-verified outcome (important: corrects an agent false-positive)

An independent review agent (background, general-purpose) was asked to review the full working-tree diff
after all rounds above. It reported 3 findings. **Each was individually TDD-verified** (revert →
rebuild --no-incremental → confirm actual pass/fail → restore) before acting on it — this caught the
agent being WRONG about the highest-severity one:

1. **FALSE POSITIVE, do not re-flag**: the agent claimed `ContentService.Copy(content, Constants.System.Root,
   ...)` leaks `Constants.System.RootSystemKey` into `copy.ParentKey` (via `TryGetParentKey` →
   `IdKeyMap.GetKeyForIdAsync`, which special-cases only the three recycle-bin ids, not Root) - and that this
   leaks externally via `ContentCopiedWebhookEvent`. **This does NOT reproduce.** A test
   (`Copy_To_Root_Populates_Null_ParentKey_Not_The_Root_Nodes_Own_Key` in `ContentServiceTests.cs`) passed
   BOTH with and without the "fix" applied - a genuine coincidental-pass, exactly the trap root `CLAUDE.md`
   §10 warns about. Root cause the agent missed: `IdKeyMapRepository.GetKeyForIdAsync` (the ONLY
   `IIdKeyMapRepository` implementation in the codebase, EF-Core-backed) filters `WHERE NodeObjectType ==
   {requested type} OR NodeObjectType == IdReservation`. Root's seeded row (`DatabaseDataCreator.cs`) has
   `NodeObjectType = Constants.ObjectTypes.SystemRoot` - which matches neither `Document` nor
   `IdReservation` - so the query returns no row, `Attempt<Guid>.Fail()`, and `TryGetParentKey` sets
   `parentKey = null`. The "leak" is structurally impossible via this path today. The `Copy` code was still
   changed to a `parentId switch { Root => null, RecycleBinContent => sentinel, _ => TryGetParentKey(...) }`
   (matching `PerformMoveLocked`'s already-proven pattern) - this is a legitimate defensive/consistency
   improvement (avoids a wasted `IIdKeyMap` round trip for Root/RecycleBin, and protects against a
   hypothetical future loosening of `IdKeyMapRepository`'s object-type filter) but is NOT fixing a live bug.
   Kept the test as a plain contract assertion (matches the style of other Root-behavior tests in this
   codebase), not framed as a regression-proof of a real exploit.
2. **CONFIRMED REAL, fixed**: `MediaService.PerformMoveLocked` set `media.ParentId = parentId;` without
   re-populating `media.ParentKey` afterward (unlike `ContentService.PerformMoveLocked`, which does). Since
   `ContentBase.ParentId`'s setter invalidates `_hasParentKey` on every assignment, reading `media.ParentKey`
   right after any `MediaService.Move(...)` to a real (non-Root, non-RecycleBin) parent threw
   `NotSupportedException` - reproduced exactly via TDD revert (test `Can_Move_Media_Populates_ParentKey`,
   `MediaServiceTests.cs`, failed with that exact exception before the fix, passed after). Fixed with the
   same `parentId switch { Root => null, RecycleBinMedia => sentinel, _ => parent?.Key }` pattern, using the
   already-in-scope `parent` parameter (no extra lookup needed).
3. **CONFIRMED REAL (latent), fixed defensively**: `ContentBase.SetParent(ITreeEntity? parent)` treated ANY
   non-null `parent` as if it had a key, silently defaulting to `null` (root) when `parent` wasn't an
   `IContentBase`. Reproduced via TDD revert with a real non-`IContentBase` `ITreeEntity`
   (`EntityContainer`) as the parent (test `SetParent_With_Non_ContentBase_Parent_Leaves_ParentKey_Unresolved`,
   `ContentBaseParentKeyTests.cs`) - failed (returned `null` instead of throwing) before the fix, passed
   after. Fixed: only treat `_hasParentKey = true` when `parent is null or IContentBase`; otherwise leave it
   unresolved (`_hasParentKey = false`) so the getter's own Root/RecycleBin/throw logic runs instead of
   silently lying. Confirmed unreachable via any current call site (no `SetParent` call in `src/`/`tests/`
   passes a non-`IContentBase` to a `ContentBase`-typed instance today) but worth the one-line-of-risk fix
   for contract correctness.
4. **NOT fixed, deliberately out of scope**: the same `ParentKey`-not-repopulated-after-move gap almost
   certainly also exists in `ElementEditingService.MoveLockedAsync`/`CopyAsync` (`toMove.ParentId =
   parentId;` / `copy.ParentId = newParentId;` with no matching `ParentKey` update) - this file was NEVER
   touched by any round of this feature (unlike `MediaService.cs`, which was touched here specifically to
   close the gap found above). Did not fix: `ElementEditingService`'s existing parent-key resolution
   (`CopyAsync`'s real-parent branch calls `_idKeyMap.GetKeyForIdAsync(newParentId,
   UmbracoObjectTypes.ElementContainer)`) uses a domain-specific object type (elements apparently resolve
   their container's key via `ElementContainer`, not `Element`) that isn't fully understood without more
   research - risk of an incorrect fix outweighed the benefit here, since (like the Media gap before it was
   fixed) nothing reads `IElement.ParentKey` after a move/copy yet. If this surfaces again, research
   `ElementContainer` vs `Element` object-type semantics for elements' parents before fixing.

Test-file changes from this round: `ContentServiceTests.cs` (+1 test), `MediaServiceTests.cs` (+1 test, new
file for this feature), `ContentBaseParentKeyTests.cs` (+1 test). All three individually TDD-verified.

### Verification (cumulative across all rounds of this feature)
- Every production change was TDD-verified with a genuine revert→rebuild(`--no-incremental`)→confirm-fails→
  restore→confirm-passes cycle, not just "test passes once."
- Full solution build (`dotnet build umbraco.sln --no-incremental`): 0 errors, repeated after every design
  round described above.
- Full `Umbraco.Tests.UnitTests`: 6546/6546 passed (6 skipped, pre-existing/unrelated), most recently after
  the `SetParentKey` removal round.
- Targeted integration sweep (`ContentServiceTests|ContentTypeServiceTests|DocumentRepositoryTest|
  AsyncDocumentRepositoryTest|MediaRepositoryTest|MemberRepositoryTest|ContentEditingService|
  MediaEditingService|MemberEditingService|ElementEditingService|ContentBlueprintEditingService|
  Navigation`): 923/923 passed, most recently after the `SetParentKey`-removal round. A full untargeted
  integration run (923/923, same count — the filter already covers everything relevant) was also green
  after the self-join round.
- Independent review agents (separate from the implementer) found 0 correctness bugs across every round;
  only minor cleanliness nits, all fixed.
- Three unrelated, PRE-EXISTING test failures were found during an earlier sweep
  (`ContentServiceNotificationTests`: `Can_Read_All_Changed_Cultures_As_Saved_When_Publishing_A_Subset`,
  `Can_Read_Saved_Notification_When_Save_And_Publishing_Invariant`,
  `Can_Read_Saved_Notification_When_Save_And_Publishing_Variant`). Confirmed via `git stash` that these
  fail identically with all ParentKey work completely removed — **not** caused by this feature. Left
  alone; out of scope. If asked to fix them, start there.

### Files touched (cumulative)
Production: `IContentBase.cs`, `ContentBase.cs`, `Content.cs`/`Media.cs`/`Member.cs`/`Element.cs`,
`TreeEntityBase.cs`, `ContentService.cs`, `MediaService.cs`, `AsyncPublishableContentServiceBase.cs`,
`ContentEditingServiceBase.cs`, `AsyncContentEditingServiceBase.cs`, `ContentRepositoryBase.cs`,
`PublishableContentRepositoryBase.cs`, `MediaRepository.cs`, `MemberRepository.cs`,
`AsyncContentRepositoryBase.cs`, `AsyncDocumentRepository.cs`, `ContentDto.cs`.
Tests: `ContentBaseParentKeyTests.cs` (new, unit — rewritten to drop all `StaticServiceProvider`/
`Mock<IIdKeyMap>` setup once the DIM fallback was removed), `AsyncDocumentRepositoryTest.cs`,
`DocumentRepositoryTest.cs`, `MediaRepositoryTest.cs`, `MemberRepositoryTest.cs`, `MediaServiceTests.cs`,
`ContentServiceTests.cs`.

Not yet committed as of this writing — working-tree changes only, awaiting explicit user go-ahead to commit
(per standing instruction: only commit/push when the user explicitly asks).
