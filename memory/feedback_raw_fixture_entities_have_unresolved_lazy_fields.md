---
name: raw-fixture-entities-have-unresolved-lazy-fields
description: "When spy-testing that a service method avoids a fallback by using an entity's already-populated lazy field, pass an entity freshly re-fetched through the repository, not the raw in-memory fixture object - and measure a call-count delta around just the method under test, not an absolute zero"
metadata:
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-20T08:18:09.554Z
---

While TDD-verifying that `AsyncPublishableContentServiceBase.GetParent(TContent?)` was simplified to
read the already-populated `IContentBase.ParentKey` instead of calling `IIdKeyMap.GetKeyForIdAsync`
directly, a spy-based test that called `ContentService.GetParent(Subpage)` — passing the raw in-memory
fixture object created by `UmbracoIntegrationTestWithContent`'s setup — failed to discriminate: it
recorded exactly one `IIdKeyMap` call whether the production simplification was in place or reverted.

Root cause: the raw fixture object was built via a content builder that assigns `ParentId` directly
(an int), which — per [[feedback_efcore_notracking_requires_astracking]]'s sibling design in this same
`ParentKey` effort — *invalidates* any previously-set `ParentKey` (`_hasParentKey = false`). So on
first access, `content.ParentKey`'s own default-interface-implementation fallback calls `IIdKeyMap`
itself, regardless of whether the *service* method still has its own separate `IIdKeyMap` call. This is
the same "wrong door" class of bug as [[feedback_spy_the_actual_fallback_seam_not_the_injected_one]],
but the trigger here is different: not an ambient resolution path, but an entity instance that never
went through the repository's batch `ParentKey` population.

**Why:** A fixture object built directly by a test (not re-fetched via `GetByIdAsync`/a repository
query) does not have `ParentKey` pre-populated the way a real read path populates it. Any call to
`.ParentKey` on such an object triggers the fallback exactly once, which is indistinguishable from
"the method under test called `IIdKeyMap` directly" if the assertion is an absolute zero.

**How to apply:** When spy-testing that a method reads an already-populated lazy/cached field instead
of resolving it itself: (1) obtain the entity via the same read path production code would use
(`await service.GetByIdAsync(key, ...)`), not the raw fixture instance; (2) capture the spy's call
count *after* that fetch and *before* invoking the method under test, then assert the count is
unchanged (a delta of zero) rather than asserting an absolute zero — this stays correct even if the
fetch itself makes legitimate incidental calls unrelated to the method under test.
