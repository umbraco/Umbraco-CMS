---
name: stacked-pr-after-squash-merge-use-ours-strategy
description: "When a stacked PR branch conflicts with its base after the PR below it was squash-merged, verify the squash tree equals the lower PR's head and merge the base with -s ours; -X ours duplicates hunks"
metadata:
  type: feedback
---

A branch cut from another PR's branch conflicts with the integration branch as soon as that lower PR is
squash-merged, whenever the upper branch edited the same lines further (20-10-2026, #24203 after #24175):
from the merge-base both sides change the same regions and git cannot see that one side contains the other.

**Why:** the squash commit is a different commit with identical content; three-way merge compares content
against the base, not against the sibling branch.

**How to apply:**
1. Prove the squash is a pure copy: `git diff <squash-sha> origin/<lower-pr-branch> --stat` must be empty and
   `git log <cut-point>..origin/<lower-pr-branch>` must be empty (the lower PR got nothing after the cut).
2. Then `git merge -s ours origin/<integration>` and verify `git diff <previous-tip> HEAD --stat` is empty.
3. Do not use `-X ours`: it still applies "theirs-only" hunks and duplicated a 240-line block that the upper
   branch had restructured. If step 1 is not empty, resolve by hand instead.
Related: [[project_ef_core_element_repository_campaign]].
