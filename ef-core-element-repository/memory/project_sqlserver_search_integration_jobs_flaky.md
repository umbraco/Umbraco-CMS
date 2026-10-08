---
name: sqlserver-search-integration-jobs-flaky
description: The Azure "Search Integration Tests (SQL Server)" Linux/Windows jobs fail on every branch that runs them (path-id, culture, sort, ancestor/children selector tests); v19/dev and the ef-core-repositories branch never run them, so there is no green baseline
metadata:
  type: project
---

As of 2026-09-25, Azure builds 288393 (this PR) and 287628/287142 (v19/feature/ef-core-media-type-repository) all fail the SQL Server search jobs with overlapping, run-to-run varying sets: CanFilterByPathIds, ByCulture_CanFilterByPathIds, PublishedStructureInAllCultures_WithUnpublishedRootInSingleCulture, CanSortKeywords/CanSortDecimals, AncestorsSelector/ChildrenSelector/DescendantsSelector. v19/dev and the target branch only run the SQLite search jobs.

**Why:** the failures look like index timing/state leaking between tests on the slower SQL Server runners, not code under test; treating them as PR regressions wastes time.

**How to apply:** before investigating a red SQL Server search job, diff its failed-test list against those builds (Azure REST: builds → timeline → "Run dotnet test" log). Only tests that fail on this PR and NOT on those baselines are the PR's problem. Timeline/log endpoints are public; the test-runs endpoint needs auth.
