---
name: verify-investigation-agent-negative-claims
description: "Before acting on an investigation agent's \"X doesn't exist\"/\"nothing else calls this\"/\"fully migrated\" claim in a way that could break compilation, do a quick direct grep yourself rather than fully trusting it."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-27T07:56:16.526Z
---

When a parallel investigation agent reports a **negative** finding — "this interface member isn't
separately redeclared elsewhere," "production code is fully migrated, nothing left to do," "no other
caller exists" — treat it as a strong lead, not a verified fact, before taking an action that depends
on it being exactly right (like deleting an interface member or declaring a migration complete).

**Why:** happened twice in the same increment (retiring `IContentService`'s sync `Save`). (1) An agent
claimed `IContentService.Save(IContent, ...)` was purely inherited from the shared
`IPublishableContentService<TContent>` interface, not separately redeclared — wrong; `IContentService.cs`
had its own identical, un-`new`-marked redeclaration. Removing only the shared-interface copy compiled
fine at first glance but left the method fully exposed. Caught immediately by a real build (`CS0535`
the moment the *other* copy was also removed), not by the agent's own analysis. (2) A separate agent
claimed all production `src/` callers of a soon-to-be-deleted sync method were already migrated, found
only 2 remaining call sites — wrong; the compiler surfaced 4 more in files the grep-based investigation
missed entirely once the method was actually deleted.

**How to apply:** negative/completeness claims from investigation agents are exactly the kind of thing
a real compiler check catches for free the moment you make the corresponding change — so the practical
mitigation isn't "spend more agent budget re-verifying before touching anything," it's: make the change,
rebuild immediately, and treat any resulting compile error as the investigation continuing, not as a
mistake to be embarrassed about. Don't be surprised when an agent's "nothing else references this" is
incomplete — budget a build-and-fix-fallout pass into the plan for any interface/signature removal,
rather than assuming the investigation phase found every caller.
