# Code Review: PR #22217 -- Change the default webhook payload type to "minimal"

**Branch target:** `v18/dev`
**Files changed:** 3 (12 insertions, 13 deletions)

---

## Summary

This PR makes three related changes for Umbraco 18:

1. **Switches the default webhook payload type** from `Legacy` to `Minimal` in `Constants.Webhooks.DefaultPayloadType`.
2. **Extends the `Legacy` enum deprecation timeline** from "removal in Umbraco 18" to "removal in Umbraco 19", giving users one more major version to migrate.
3. **Updates the telemetry provider** to use `Constants.WebhookEvents.Aliases.*` constants instead of hardcoded strings, and adds three new Element webhook event types (`ElementDelete`, `ElementPublish`, `ElementUnpublish`) to the default telemetry tracking.

The intent is clear and well-scoped: the default was always planned to switch to `Minimal` in v17/v18 (the existing comments say so explicitly), and the `Legacy` removal schedule is appropriately extended to give a full deprecation period.

---

## File-by-File Analysis

### 1. `src/Umbraco.Core/Constants-Webhooks.cs`

**Change:** Default payload type constant changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`. The `<remarks>` XML doc block explaining the planned migration is removed.

| Severity | Finding |
|----------|---------|
| **Low -- Stale doc comment** | The `<summary>` on line 13 says "Gets the default webhook payload type" which is still accurate. However, the removal of the `<remarks>` block is appropriate since the planned switch is now completed. No issue here. |

**Verdict:** Clean change. Follows through on the documented plan.

### 2. `src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`

**Change:** The `Legacy` enum member's `<remarks>` and `[Obsolete]` attribute are updated. The remark now says "available as a configurable option for Umbraco 17 and 18" (was "Umbraco 17"), and removal is pushed from v18 to v19.

| Severity | Finding |
|----------|---------|
| **OK** | The `[Obsolete("Scheduled for removal in Umbraco 19.")]` follows the repo's convention. Since `Legacy` was introduced/obsoleted in v16 and the minimum deprecation window is one full major version before removal, removal in v19 (giving the whole of v18 as the deprecation period with the new default) is correct. |
| **Medium -- Stale doc comment on `Minimal`** | Line 12 of the current file (not changed by this PR) reads: `/// Expected to be the default option from Umbraco 17.` Since `Minimal` is now *actually* the default in v18, this remark is stale and slightly misleading. It should either be removed or updated to say something like "This is the default option from Umbraco 18." This is a pre-existing issue that the PR should clean up while it is already modifying this file. |

### 3. `src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`

**Change:** The `_defaultEventTypes` array is updated in two ways: (a) magic strings replaced with `Constants.WebhookEvents.Aliases.*` references, and (b) three new Element event types added.

| Severity | Finding |
|----------|---------|
| **OK -- Good improvement** | Replacing `"Umbraco.ContentDelete"` etc. with `Constants.WebhookEvents.Aliases.ContentDelete` eliminates magic strings and is consistent with the codebase's use of the `Constants` class. This is a quality improvement. |
| **OK -- Trailing comma** | The trailing comma after `Constants.WebhookEvents.Aliases.ElementUnpublish` on line 54 of the diff is valid C# collection expression syntax and consistent with the collection expression style used. |
| **Info -- New Element events are v18-specific** | The `ElementDelete`, `ElementPublish`, and `ElementUnpublish` aliases do not exist in the v17 codebase (confirmed via search). They are v18 additions related to the "Global Elements" feature (visible in the git log: PR #21875, #21877, #22012). This is appropriate for the `v18/dev` target branch. |
| **Low -- No test coverage** | There are no unit tests for `WebhookTelemetryProvider` in the test projects. The `_defaultEventTypes` array also affects the "custom webhooks" count (line 52 of the provider), meaning the addition of Element events changes which webhooks are counted as "custom" vs "default." This is not introduced by this PR (the lack of tests is pre-existing), but it is worth noting. |

---

## Cross-Cutting Concerns

### Breaking Change Assessment

| Severity | Finding |
|----------|---------|
| **Medium -- Behavioral breaking change for upgraders** | Changing the `DefaultPayloadType` constant from `Legacy` to `Minimal` means that users upgrading from v17 to v18 who have not explicitly set a `PayloadType` in their `WebhookSettings` configuration will silently receive a different payload format. This is intentional and pre-announced, but the PR should ensure that the v18 upgrade/migration documentation calls this out prominently. The constant flows through `WebhookSettings.cs` (line 22: `StaticPayloadType`) and `UmbracoBuilder.Collections.cs` (line 97) to affect all webhook registrations. |
| **Medium -- Stale doc in `WebhookSettings.cs`** | `/src/Umbraco.Core/Configuration/Models/WebhookSettings.cs` lines 82-84 still say: `"By default, Legacy payloads are used... This default will change to minimal starting from v17."` This is now incorrect for v18 -- it should say that `Minimal` payloads are used by default. This is not changed by the PR but should be updated since the PR is the one making these comments false. |

### Obsolete Pattern Compliance

The `[Obsolete("Scheduled for removal in Umbraco 19.")]` on `Legacy` is well-formed and follows the repository's convention. Pushing the removal by one version is reasonable given that the default is only now switching.

### Impact Scope

The `DefaultPayloadType` constant is referenced in 4 locations:
- `Constants-Webhooks.cs` (definition) -- changed
- `WebhookSettings.cs` line 22 -- uses constant for config default
- `UmbracoBuilder.Collections.cs` line 97 -- uses constant for webhook event registration
- `WebhookEventCollectionBuilderExtensions.cs` line 19 -- uses constant as default parameter

All downstream usages correctly reference the constant, so changing the constant value propagates correctly everywhere. No manual updates needed at call sites.

---

## Issues Summary

| # | Severity | File | Description |
|---|----------|------|-------------|
| 1 | **Medium** | `WebhookPayloadType.cs:12` | Stale `<remarks>` on `Minimal`: "Expected to be the default option from Umbraco 17" -- should be updated since it is now the default in v18. |
| 2 | **Medium** | `WebhookSettings.cs:82-84` | Stale `<remarks>` on `PayloadType` property: still says "Legacy payloads are used" and "This default will change to minimal starting from v17." Should be updated to reflect the new default. |
| 3 | **Low** | General | No unit test coverage for `WebhookTelemetryProvider`. Pre-existing, but this PR changes the behavior of the custom-vs-default webhook categorization. |

---

## Verdict: **Approve with minor changes requested**

The core change (switching the default) is correct, well-planned, and cleanly executed. The `[Obsolete]` schedule extension is appropriate. The telemetry cleanup (magic strings to constants + adding Element events) is a welcome improvement.

The two medium-severity items are stale documentation comments in files that this PR directly affects or whose behavior it changes. They should be updated in this PR to avoid leaving behind incorrect documentation. Both are small one-line fixes.
