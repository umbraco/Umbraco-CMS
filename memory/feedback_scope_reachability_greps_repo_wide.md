---
name: feedback-scope-reachability-greps-repo-wide
description: "Does anything still call X?" must be grepped repo-wide, never scoped to the file X lives in
metadata:
  type: feedback
---

Before claiming a member is dead, unused, or has a "sole remaining caller", grep the **whole repo** — never
just the file the member is declared in. Scope the search to the question (reachability, which is global),
not to the code you happen to be reading.

**Why:** planning the `PerformScheduledPublish` retirement, I grepped
`AsyncPublishableContentServiceBase.cs` for callers of `CommitContentChangesInternal`, found only the method
I was about to convert, and wrote "this is the sole remaining caller of the sync engine" into the plan —
along with a second commit that would delete ~600 lines. But the member is `protected`, so **subclasses call
it from other files**: `ContentService.cs` did, twice (`PublishBranchItem`, and an `internal`
`CommitDocumentChanges` with test-only callers). The deletion failed to build and had to be reverted, and
the roadmap changed — the deletion moved to a later increment.

**How to apply:** for any "is this still used / can this go" question, run
`git grep -n "MemberName(" -- '*.cs'` across the repo before writing the conclusion down. Accessibility is
the tell: `private` is the only modifier where a single-file grep answers the question. `protected`,
`internal`, `public` and interface members are all reachable from elsewhere *by design* — that is what they
are for.

Corollary: a build is the real authority, a grep is a hypothesis. When the plan depends on "nothing else
references this", sequence the work so the compiler tests that claim early and cheaply rather than after
you have written the deletion. See [[project_document_repository_retirement_plan]] and
[[feedback_verify_code_path_actually_executes]].
