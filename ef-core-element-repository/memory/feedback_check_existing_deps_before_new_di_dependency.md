---
name: check-existing-deps-before-new-di-dependency
description: "Before adding a new constructor dependency to resolve some type (e.g. int-to-Guid-key lookup), grep the target class's already-injected fields and neighboring methods first — it may already have a dependency that does the job."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-11T10:59:32.931Z
---

Before adding a new constructor parameter/DI dependency to a class to solve a small resolution problem
(e.g. "I need to turn this int id into a Guid key"), grep that class's *existing* private fields and
sibling methods first for something that already does it — don't assume a new dependency is needed just
because the immediate method doesn't have one in scope.

**Why:** Caught during the `Rollback` retirement ([[project_document_repository_retirement_plan]]):
added `IIdKeyMap` as a brand-new constructor param to `ContentVersionServiceBase`, threaded through both
its subclasses (`ContentVersionService`, `ElementVersionService`) — three files touched, new DI surface
added. User asked "is that necessary, can we do something else?" — a two-minute grep of the same class
showed `_entityService` (`IEntityService`) was already injected, and `_entityService.GetKey(int,
UmbracoObjectTypes).Result` was already the established pattern in that exact file (used twice already,
in the version-cleanup path) for the identical int→Guid resolution. The "add a new dependency" instinct
was reasonable in isolation but skipped a cheap check that would have avoided it entirely.

**How to apply:** Whenever the task is "resolve/convert/look up X" inside an existing class, before
reaching for a new interface/constructor param: (1) grep the class's own field list, (2) grep the class's
own method bodies for the same conversion done elsewhere. Only add a new dependency if neither turns up
anything. This is the caller-side analogue of the campaign's "reuse existing patterns" discipline that
already applies to the retired member's own body — it applies just as much to the plumbing that calls it.
