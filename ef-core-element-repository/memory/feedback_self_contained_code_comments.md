---
name: feedback-self-contained-code-comments
description: "Code comments and TODOs must never reference ephemeral session artifacts (plan files under ~/.claude/plans/, this conversation) — only real, durable repo content"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-04T13:31:45.538Z
---

While implementing the `AsyncDocumentRepository` write path, several TODO/explanatory comments were initially written as "...see the write-path plan" — referencing the local plan file at `~/.claude/plans/atomic-petting-barto.md`. That file is a per-session planning artifact, not part of the repository; it won't exist for another developer, another session, or even this same session after the plan file is cleaned up.

**Why:** Caught this while doing a follow-up "add TODOs for deferred items" task — had to go back and rewrite every comment that referenced "the write-path plan" into something self-contained (pointing at real NPoco class/method names that exist in the repo, e.g. "see NPoco `PublishableContentRepositoryBase.PersistNewItem`" instead of "see the write-path plan").

**How to apply:** When writing any code comment or TODO that explains *why* something is scoped down, deferred, or diverges from a reference implementation, point only at things that will still be resolvable by a future reader with just the repo checked out: other files/classes/methods in the same repo, external issue trackers, or a plain-English explanation inline. Never reference: a local plan file path, "this conversation," "the session," or anything under `~/.claude/`. This applies retroactively too — if an earlier pass in the same piece of work left a comment like this, fix it when noticed, don't leave it for later.
