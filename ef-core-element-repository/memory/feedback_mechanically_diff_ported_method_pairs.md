---
name: feedback-mechanically-diff-ported-method-pairs
description: "Verify a sync→async (or any duplicated) method port with a normalising diff script, not by reading"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-16T08:25:08.459Z
---

When porting a long method to an async twin that must stay behaviourally identical, prove faithfulness by
extracting both bodies and diffing them after normalising away the *expected* differences — don't read them
side by side and don't rely on a reviewer's assurance.

**Why:** on the ~370-line `CommitContentChangesInternal` → `CommitContentChangesInternalAsync` port, a ~40
line Python script (find signature → brace-match the body → strip blank lines → `s/await //`, `s/FooAsync/Foo/`,
map each repo-call swap and `.GetAwaiter().GetResult()` unwrap to a shared token → `difflib.unified_diff`)
settled in seconds what three separate review passes each spent significant effort asserting. It reported
`StrategyCanPublishAsync` **identical over 146 normalised lines**, `StrategyCanUnpublishAsync` identical, and
`CommitContentChangesInternalAsync` differing *only* by `cancellationToken` threading — a far stronger claim
than "I read it and it looks faithful", and it surfaced the one genuine difference (an omitted dead guard)
without any judgement call.

**How to apply:** write the script before reading the diff. Each normalisation you add is an explicit,
reviewable statement of "this difference is expected" — which is exactly the list you want in the commit
message and the review brief anyway. Any line the script still flags is either a real defect or a deliberate
deviation worth calling out. Keep the pair diffable until the old method is deleted; see
[[project_document_repository_retirement_plan]].
