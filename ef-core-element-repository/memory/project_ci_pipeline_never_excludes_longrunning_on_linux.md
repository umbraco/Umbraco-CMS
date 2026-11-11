---
name: ci-pipeline-never-excludes-longrunning-on-linux
description: "Azure pipeline bug: the LongRunning/NonCritical category exclusion never applies to Linux/macOS integration jobs, and ADO build logs are readable anonymously"
metadata:
  node_type: memory
  type: project
  originSessionId: 83ff3bb1-6ad5-4778-b26e-0a7c2f58c918
  modified: 2026-11-11T09:41:17.266Z
---

`build/azure-pipelines.yml` picks the integration-test filter with a compile-time template expression (`${{ elseif or(variables.releaseTestFilter, parameters.forceReleaseTestFilter) }}`). `releaseTestFilter` is the non-empty literal string `eq(dependencies.Build.outputs[...])`, so the expression is always truthy and every Linux/macOS integration job runs with the bare part filter: `TestCategory!=LongRunning&TestCategory!=NonCritical` is never applied there. Verified in build 300747's log (11-11-2026). `[LongRunning]` is `Umbraco.Tests.Common/Attributes/LongRunning.cs`.

Azure DevOps build logs for this repo ARE readable without signing in (contrary to an earlier assumption): `https://dev.azure.com/umbraco/Umbraco%20Cms/_apis/build/builds/<id>/timeline` and `.../logs/<logId>`, plus `vstmr.dev.azure.com/.../testresults/resultsbybuild?buildId=<id>&outcomes=Failed`. Only `_apis/test/runs` redirects to sign-in. Build ids come from `gh pr checks <n> --json name,state,link`.

**Why:** Reproducing a CI part locally with the category exclusion added gives a false green (Part 2 was green locally, red on CI, because `ThreadSafetyServiceTest` is `[LongRunning]`). And pulling the actual failed-test list from the log beats inferring it.

**How to apply:** When reproducing a non-Windows integration job locally, use exactly the part filter from lines ~353-376 of the pipeline, with no category exclusion. Read the CI log first to get the exact failure list. Related: [[integration-branch-regressions-after-0911-mergeup]], [[sqlserver-search-integration-jobs-flaky]].
