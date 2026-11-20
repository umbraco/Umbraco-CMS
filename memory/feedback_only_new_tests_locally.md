---
name: feedback_only_new_tests_locally
description: Run only the tests a change adds or directly touches locally; the user runs full fixtures and suites on the pipeline (13-10-2026, reaffirming an earlier "only targeted tests" note)
metadata:
  type: feedback
---

On 13-10-2026, while verifying a new ordering test for PR 2, I started the full
`DocumentRepositoryTest` fixture on SQLite and SQL Server. The user stopped it: "Don't run all
integration tests, only the new ones, I'll run the rest on the pipeline."

**Why:** the fixtures take many minutes locally and the pipeline runs them anyway on push; local
time is better spent proving the new test can fail (break the code, see red, restore).

**How to apply:** after adding or changing a test, run that test (plus the handful it sits next to
if they share setup) and the red/green proof, then push and let CI run the rest. Do not launch
whole fixtures or the broad integration filter unless the user asks. See also
[[feedback_parallel_review_and_test_agents]] for the review step, which stays.
