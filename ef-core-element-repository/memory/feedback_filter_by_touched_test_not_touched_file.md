---
name: filter-by-touched-test-not-touched-file
description: "Integration test filters must target the touched TEST methods, not the whole fixtures containing them - a file-level filter drags in huge fixtures where only one test changed."
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-21T07:47:59.059Z
---

When verifying a change that touches many integration fixtures, build the `--filter` from the **test
methods the diff actually lands in**, not from the fixture names. A file-level filter pulls in entire
fixtures — `ContentServiceTests` alone is hundreds of tests — when a single test in them changed.

**Why:** the user called this out directly ("Running all the integration tests takes way too long, you
should only run a targeted set"). A nine-fixture file-level filter ran for minutes and had to be killed;
the equivalent method-level filter was **58 tests in 38 seconds**.

**How to apply:**
1. `git diff -U0 -- <file> | grep "^@@" | sed 's/.*@@ //'` prints the enclosing member for every hunk —
   that is the list of touched tests, for free.
2. Name those tests in the filter where the fixture is large.
3. Fall back to the whole fixture only when it is small *or* a shared `[SetUp]`/helper changed, so most
   of its tests are genuinely affected.

**Reinforced 2026-09-23**, verbatim: *"Don't run ALL tests, you'll never finish only run targeted
tests."* Said after a filter of `DocumentRepositoryTest|DocumentBlueprintRepositoryTest|ContentService|
RecycleBin|Permission` ran ~900 tests for 10+ minutes and had to be killed twice — it also held a file
lock that blocked every rebuild, stalling all other work. A behaviour-level filter over the same change
(`VersionsSlim|GetVersionIds|Ordering|RecycleBin|SaveAsync_Caches|...`) was **190 tests in 2 minutes**.
Prefer naming behaviours/methods over fixtures even when several fixtures are touched. Long runs are not
free: kill them before rebuilding, and `taskkill //F //IM testhost.exe` clears a stale lock.

Refines [[targeted-tests-suffice-skip-full-suite]], which said "a filter covering every touched file" —
that is still too coarse for this repo's larger fixtures.
