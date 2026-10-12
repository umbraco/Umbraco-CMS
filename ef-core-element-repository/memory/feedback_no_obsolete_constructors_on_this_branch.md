---
name: no-obsolete-constructors-on-this-branch
description: "On this branch, constructor changes can be breaking too — don't apply the CLAUDE.md §6.1 obsolete-constructor-forwarding pattern; just add the new parameter directly."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-25T12:02:32.236Z
---

Don't apply root CLAUDE.md §6.1's "obsolete constructor + StaticServiceProvider" pattern on this
branch (`v18/feature/ef-core-document-repository`) when a class needs a new constructor dependency.
Just add the new parameter directly to the single constructor — no `[Obsolete]` old-constructor
shim, no `StaticServiceProvider.Instance.GetRequiredService<T>()` forwarding.

**Why:** the user said explicitly: "You don't have to make an obsolete[r] constructor, you can make
breaking changes here." This branch targets an unreleased `19.0.0-beta1` and already treats interface
members, notification shapes, etc. as freely breakable (see [[project_document_repository_retirement_plan]]
and the notification Id→Key migration) — that same license extends to constructors, not just
interfaces/method signatures. Caught after already adding a full obsolete-constructor chain to
`ContentService` (modeled on `UserPresentationFactory.cs`) for the `GetBlueprintByIdAsync` increment;
had to strip it back down to a single, directly-modified constructor.

**How to apply**: on this branch specifically, when a class needs a new constructor dependency, add
the parameter straight to the existing constructor and update the field-assignment list — don't create
a second, obsolete-marked overload. This applies broadly across the campaign's remaining increments
(any future `ContentService`/`MediaService`/etc. constructor change), not just this one instance. Note:
[[feedback_check_document_specific_repo_before_generic_base]] mentioned `MediaService` gaining
`IIdKeyMap` via the obsolete-forwarding pattern in an *earlier* increment — that one should also have
been a direct breaking change per this same guidance; only revisit it if touching that code again, no
need to proactively go back and simplify it.
