# Code Review: PR #22217 - Change the default webhook payload type to "minimal"

**Branch**: pr/22217 targeting `v18/dev`
**Commits**: 2 commits (`1a316ac` and `0538555`)
**Files Changed**: 3

---

## Summary

This PR makes a planned change to switch the default webhook payload type from `Legacy` to `Minimal`, fulfilling a deprecation plan originally set up in v16. It also extends the `Legacy` enum member's removal schedule from v18 to v19 (giving an extra major version of grace period), and updates the telemetry provider to use constants instead of magic strings while also adding Element event types to the tracked defaults.

---

## File-by-File Analysis

### 1. `src/Umbraco.Core/Constants-Webhooks.cs`

**Change**: Default payload type changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`. Outdated remarks removed.

**Assessment**: Clean and correct. The `DefaultPayloadType` constant flows correctly into:
- `WebhookSettings.StaticPayloadType` (line 22 of `WebhookSettings.cs`)
- `UmbracoBuilder.Collections.cs` (line 97) for DI registration
- `WebhookEventCollectionBuilderExtensions.AddCms` default parameter

No issues.

### 2. `src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`

**Change**: The `Legacy` enum member's `[Obsolete]` message updated from "Scheduled for removal in Umbraco 18" to "Scheduled for removal in Umbraco 19". XML remarks updated correspondingly.

**Assessment**: Correct. Since this is v18 and removing `Legacy` now would be a breaking change for anyone relying on it, extending the removal to v19 is the right call. The original deprecation was in v16, so the N+2 rule would have allowed removal in v18, but extending it further is acceptable and more conservative.

### 3. `src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`

**Change**:
- Magic strings (`"Umbraco.ContentDelete"`, etc.) replaced with `Constants.WebhookEvents.Aliases.*` constants.
- Three new Element event types added: `ElementDelete`, `ElementPublish`, `ElementUnpublish`.

**Assessment**: Good improvement. The constants match their string values exactly (verified against `Constants-WebhookEvents.cs`). The new Element events align with the default webhook events registered for `Minimal` payload type (the `_defaultTypes` array in `WebhookEventCollectionBuilderCmsExtensions.cs`), so the telemetry tracking now matches the actual defaults.

---

## Issues Found

### Stale Documentation (Low Severity)

**1. `WebhookPayloadType.Minimal` has an outdated remark**

File: `src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`, lines 11-13

```csharp
/// <remarks>
/// Expected to be the default option from Umbraco 17.
/// </remarks>
```

This remark says "Expected to be the default option from Umbraco 17" but this PR is making it the default in v18, not v17. Since `Minimal` is now *actually* the default (as of this PR), the remark should be updated to reflect reality, e.g. "Default option from Umbraco 18 onwards." or simply removed, since it describes a plan that has now been executed.

**2. `WebhookSettings.PayloadType` has an outdated remark**

File: `src/Umbraco.Core/Configuration/Models/WebhookSettings.cs`, lines 80-84

```csharp
/// <remarks>
///     <para>
///         By default, Legacy payloads are used see <see cref="WebhookPayloadType"/> for more info.
///         This default will change to minimal starting from v17.
///     </para>
/// </remarks>
```

This remark says "By default, Legacy payloads are used" and "This default will change to minimal starting from v17." Both statements are now incorrect -- the default *is* now Minimal, and it changed in v18, not v17. This should be updated to accurately describe the current behavior.

### Inconsistent Default Parameter Values (Low Severity, Pre-existing)

Multiple extension methods in the `WebhookEventCollectionBuilderCms*Extensions` files use `WebhookPayloadType.Legacy` as their default parameter value rather than `Constants.Webhooks.DefaultPayloadType`:

- `WebhookEventCollectionBuilderCmsExtensions.AddDefault(..., WebhookPayloadType payloadType = WebhookPayloadType.Legacy)`
- `WebhookEventCollectionBuilderCmsContentExtensions.AddDefault(...)`
- `WebhookEventCollectionBuilderCmsMemberExtensions.AddDefault(...)`
- `WebhookEventCollectionBuilderCmsElementExtensions.AddDefault(...)`
- And many more (approximately 30+ methods).

**Impact**: This is a pre-existing issue not introduced by this PR, and the internal code path always passes the payload type explicitly (via `UmbracoBuilder.Collections.cs` line 108). However, any external consumer calling these extension methods without specifying the `payloadType` parameter would get `Legacy` behavior instead of the new `Minimal` default. Since `WebhookPayloadType.Legacy` is marked `[Obsolete]`, these default values will also produce compiler warnings at the declaration sites.

Note: Changing these defaults would itself be a binary breaking change (default parameter values are compiled into the caller), so it may be intentional to leave them as `Legacy` for backward compatibility with external packages. This is worth documenting but not necessarily worth changing.

---

## Breaking Change Assessment

**Behavioral breaking change**: Yes, this is an intentional, planned behavioral breaking change. Any existing Umbraco installation upgrading to v18 that has not explicitly configured a webhook payload type in their settings will now get `Minimal` payloads instead of `Legacy` payloads. This means:

- Webhook payloads will contain less data by default (minimal identifiers only)
- Consumers that rely on the full Legacy payload structure will need to either:
  1. Explicitly configure `"Umbraco:CMS:Webhook:PayloadType": "Legacy"` in their settings, or
  2. Update their webhook consumers to work with the Minimal payload format

This is expected for a major version bump (v16 -> v18) and was clearly planned, as the original comments stated this change would happen in v17/v18.

**Binary breaking change**: No. The public API surface (enum values, constant field, method signatures) is unchanged. Only the constant's *value* changed, and constants are inlined at compile time, so recompilation would be required anyway.

---

## Telemetry Impact

Adding Element events to the telemetry tracking is a meaningful improvement. Previously, only Content and Media events were tracked, but with the `Minimal` payload type, Element events are now part of the default set. The telemetry now accurately reflects what Umbraco registers by default, which will provide better visibility into webhook usage patterns.

---

## Verdict

**Approve with minor suggestions.** The core changes are correct, well-motivated, and properly implement a planned deprecation path. The only issues are two stale XML documentation comments (`WebhookPayloadType.Minimal` remarks and `WebhookSettings.PayloadType` remarks) that should be updated to reflect the new reality. These are cosmetic but worth fixing to avoid confusing future developers and API documentation consumers.

---

## Suggested Improvements

1. Update `WebhookPayloadType.Minimal` remarks (line 12 of `WebhookPayloadType.cs`) to reflect that it is now the default.
2. Update `WebhookSettings.PayloadType` remarks (lines 80-84 of `WebhookSettings.cs`) to state that the default is now `Minimal`.
3. Consider whether the extension method default parameter values of `WebhookPayloadType.Legacy` should be addressed in a follow-up (this is a pre-existing issue and not in scope for this PR, but worth tracking).
