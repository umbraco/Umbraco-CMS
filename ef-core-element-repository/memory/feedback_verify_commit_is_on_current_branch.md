---
name: verify-commit-is-on-current-branch
description: "git log --all -S<string> and git blame search across ALL refs, not just the current branch — verify a found commit is actually an ancestor of HEAD (git merge-base --is-ancestor) before reasoning about its presence/absence on the branch you're working on"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T11:51:29.788Z
---

While investigating a `ContentVersionDto.Key` unique-constraint bug, used `git log --oneline --all -S"public Guid Key" -- path/to/File.cs` to find the commit that added the `Key` property. Found `2a6b8d30f28` ("Add key to version dto") and, without checking which branch it actually belonged to, concluded it was part of the current branch's history — then created a new branch from `v18/feature/ef-core-repositories` and tried to apply a fix that assumed `ContentVersionDto.Key` existed there. It didn't: the property genuinely doesn't exist on that branch. The commit only exists on a *different*, downstream branch (`v18/feature/ef-core-document-repository`) that happened to be checked out earlier in the same session.

**Why:** `--all` makes `git log`/`git blame`-adjacent searches scan every ref in the repository, not just the current branch's ancestry. A commit hash found this way tells you the change exists *somewhere*, not that it's reachable from your current `HEAD`. Two sibling branches diverging from a common ancestor can each have commits the other doesn't — assuming a found commit applies to "the branch" without checking which one is a silent, easy-to-make error, especially mid-session after switching branches multiple times.

**How to apply:** After finding a commit via any `--all`-scoped search (or by generally being unsure whether a commit is in your current lineage), confirm before reasoning about it further:
```bash
git merge-base --is-ancestor <commit> HEAD && echo "IS ancestor" || echo "NOT an ancestor"
```
or `git branch --all --contains <commit>` to see exactly which branches actually have it. Do this *before* writing code that assumes the commit's changes are present, not after a compile error reveals it — the compile error is the fallback safety net, not the intended detection method.
