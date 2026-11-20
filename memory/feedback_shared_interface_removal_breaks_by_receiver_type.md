---
name: feedback-shared-interface-removal-breaks-by-receiver-type
description: Removing a member from a shared interface breaks interface-typed receivers only; concrete-typed ones keep compiling
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-16T09:21:27.378Z
---

When retiring a member from an interface that more than one entity implements, the blast radius is decided by
each call site's **receiver type**, not by which entity it belongs to. Call sites typed to the *interface*
break; call sites typed to a *concrete class* that still declares the member keep compiling silently.

**Why:** retiring sync `Publish` from `IPublishableContentService<TContent>` (shared by Document and Element)
was scoped as "Document only — Element's own sync `Publish` is untouched and still compiles". Half true.
`ElementService`'s concrete method survived, so `private ElementService ElementService => ...` fixtures
compiled fine, but `private IElementService ElementService => ...` fixtures broke, because the member was gone
from the interface. Three separate delegated agents each rediscovered this independently and each stopped,
because the brief asserted Element was unaffected.

**How to apply:** before scoping such a removal, grep the call sites for the receiver's *declared type*, not
its name — `IElementService ElementService` and `ElementService ElementService` read identically at the call
site and behave completely differently. Expect a mixed end state (some call sites migrated, some not) and
treat that as correct rather than sloppiness, as long as it tracks receiver type.

**Second consequence, same cause — orphaned `<inheritdoc/>`.** Every surviving concrete implementation that
carried `/// <inheritdoc />` for the removed member now inherits documentation from nothing, silently: it is
neither a compiler error nor a warning, so only a reader notices. This has now bitten on two consecutive
increments (`Publish`, then `Unpublish`) — both times on Element's surviving sync method. **After removing an
interface member, grep the surviving implementations for `<inheritdoc` and give each a real doc comment.**

Corollary for delegation: a brief that asserts an unverified premise ("X is unaffected") costs more than one
that flags the uncertainty, because every agent that hits the contradiction stalls on it rather than just
handling it. State what you verified and how; mark the rest as unknown. See
[[feedback_large_efcore_migration_workflow]] and [[project_document_repository_retirement_plan]].
