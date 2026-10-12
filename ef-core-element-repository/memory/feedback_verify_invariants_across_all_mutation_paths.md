---
name: verify-invariants-across-all-mutation-paths
description: When claiming two filters/predicates are equivalent because some invariant holds, check every code path that can mutate the relevant state, not just the one that prompted the check — a plausible-sounding equivalence can hold on the obvious path and quietly fail on a different one.
metadata:
  type: feedback
  originSessionId: acc3673d-daf5-4c6f-9c2e-4a53931f01bd
  modified: 2026-08-21T14:05:14.137Z
---

While migrating `ContentService.GetPagedContentInRecycleBin` to `GetPagedContentInRecycleBinAsync`, the
new implementation swapped the old filter (`content.Path.StartsWith(RecycleBinContentPathPrefix)`) for
the already-existing repository method's filter (flat `node.Trashed`). Before committing to that
substitution, I verified the two filters were equivalent by reading `ContentService.MoveToRecycleBin` →
`PerformMoveLocked` → `PerformMoveContentLocked`, confirming `Trashed` and the recycle-bin `Path` prefix
are always set together, atomically, for every node in a trashed subtree — and concluded the two filters
could never disagree.

That verification was real but incomplete: it only checked the *one* path I thought to check (trashing a
node). An independent review agent found — and I independently re-confirmed by reading the actual code —
that `ContentService.Copy()` clones an entity via `DeepCloneWithResetIdentities()` (a `MemberwiseClone()`),
which carries the source's `Trashed` value over unchanged, and never resets it (it resets `Published`
explicitly, but not `Trashed`). `PersistNewItem` then correctly recomputes `Path` from the destination
parent but never re-touches `Trashed`. So `Copy(trashedContent, liveFolder, ...)` produces a node with a
live `Path` but `Trashed = true` — a real counterexample to the "equivalence" I'd claimed, invisible to
the old Path-based filter but newly surfaced by the new Trashed-based one. See
[[project_document_repository_retirement_plan]] for the concrete bug this produced.

**Why:** Checking the path that prompted the question ("how does something end up trashed?") feels like
it answers the equivalence question, but an invariant claim ("A always implies B") is only as strong as
the *narrowest* union of every path that can produce or mutate A or B — not just the most obvious one.
`Copy` was never on my radar because the task was about trashing/recycling, not copying; the two features
just happen to share the same underlying flag.

**How to apply:** Before asserting two predicates/filters over the same entity field are equivalent (a
prerequisite for e.g. swapping which one a migration relies on), grep for every mutator of the field(s)
involved (`Trashed =`, `.Path =`, etc.) across the whole class/codebase — not just the one you already
know about — and check each one preserves the claimed relationship. If enumerating every mutator is
impractical, say so explicitly rather than presenting the narrower check as if it covered the general
case, and flag the residual risk to the user instead of asserting equivalence as settled fact.
