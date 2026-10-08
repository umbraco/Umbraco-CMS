---
name: project_async_navigation_roots_not_threadsafe
description: AsyncContentNavigationServiceBase had drifted from its sync twin on Roots and snapshot capture — realigned 2026-09-22; its mutators are now byte-identical to the base
metadata:
  type: project
---

`src/Umbraco.Core/Services/Navigation/AsyncContentNavigationServiceBase.cs` is a fork of
`ContentNavigationServiceBase`. It had drifted in four ways, all fixed 2026-09-22 (56040b87ab2,
ae068c2c50e): `NavigationSnapshot.Roots` was a plain `HashSet<Guid>` (base:
`ConcurrentHashSet<Guid>`); `Add` registered the root before the duplicate-key check; `Move`
removed the key from the roots before validating the target parent; and four mutators re-read
`_navigation`/`_recycleBinNavigation` instead of capturing each snapshot in a local and passing
it down (the three recursive helpers are now `static` and take the snapshots as parameters).

All nine mutators — Add, Move, MoveToBin, RemoveFromBin, RestoreFromBin, UpdateSortOrder and the
three recursive helpers — are byte-identical to the base as of ae068c2c50e.

**Why:** the fork was never a mechanical translation, so a difference found in one method said
nothing about the rest — three separate bugs hid behind the one the tests happened to catch.

**How to apply:** keep it identical. Two checks that need no reading:
`grep -o "_navigation\.[A-Za-z]*\|_recycleBinNavigation\.[A-Za-z]*\|ref _navigation\|ref _recycleBinNavigation"`
on both files must `diff` clean, and
`awk -v pat="    <signature>" 'index($0,pat)==1,/^    }$/'` gives a per-method pair to diff.
See [[project_async_fork_merge_hazard]] and [[feedback_mechanically_diff_ported_method_pairs]].
