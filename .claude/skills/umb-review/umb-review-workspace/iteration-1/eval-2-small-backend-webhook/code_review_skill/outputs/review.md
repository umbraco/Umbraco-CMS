# Code Review: PR #22217 - Change the default webhook payload type to "minimal"

**PR**: #22217
**Target**: `origin/v18/dev`
**Files Changed**: 3 (12 insertions, 13 deletions)

---

## Summary

This PR makes three changes for v18:

1. **Changes the default webhook payload type** from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal` in `Constants.Webhooks.DefaultPayloadType`.
2. **Extends the deprecation timeline** for `WebhookPayloadType.Legacy` from "Scheduled for removal in Umbraco 18" to "Scheduled for removal in Umbraco 19".
3. **Updates webhook telemetry** to use `Constants.WebhookEvents.Aliases.*` instead of hardcoded strings and adds three new Element event types (`ElementDelete`, `ElementPublish`, `ElementUnpublish`) to the default telemetry event tracking list.

---

## Security

**Rating: No issues**

No security concerns. The change is to default configuration values and internal telemetry. No user input handling, authentication, or authorization changes.

---

## Performance

**Rating: No issues**

The telemetry provider adds three more event types to the `_defaultEventTypes` array (from 5 to 8 entries). The additional iterations in `GetInformation()` are trivial and the `_defaultEventTypes` set is also used for the `Except` call on line 52, which remains linear in the small array size.

---

## Correctness

**Rating: 2 observations (1 informational, 1 minor suggestion)**

### 1. Stale XML doc comment on `WebhookPayloadType.Minimal` (informational)

**File**: `src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`, line 12 (on the `main` branch; pre-existing, not introduced by this PR)

The `<remarks>` on `Minimal` still reads:

```
/// Expected to be the default option from Umbraco 17.
```

Since this PR makes `Minimal` the actual default for v18, the remark is now stale. It was already outdated for v17 (v17 still defaults to `Legacy` based on the pre-PR code), and with this PR actively switching the default, keeping the old forward-looking language is misleading.

**Suggestion**: Remove or update the `<remarks>` block on `Minimal` to reflect reality, e.g., "This is the default option from Umbraco 18 onwards."

### 2. Element webhook alias constants must exist in v18 (informational)

**File**: `src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`, lines 52-54 of the diff

The PR adds references to `Constants.WebhookEvents.Aliases.ElementDelete`, `Constants.WebhookEvents.Aliases.ElementPublish`, and `Constants.WebhookEvents.Aliases.ElementUnpublish`. These constants do not exist in the v17 `Constants-WebhookEvents.cs` file, but the PR targets `v18/dev` where Elements are a new feature. This is expected to compile against the v18 codebase where those constants are defined. No action needed unless the v18 branch is missing these constants.

---

## Maintainability

**Rating: Positive with 1 minor suggestion**

### Positive: String literals replaced with constants

The replacement of hardcoded strings (`"Umbraco.ContentDelete"`, etc.) with `Constants.WebhookEvents.Aliases.*` in the telemetry provider is a good improvement. It eliminates the risk of typos and ensures consistency if aliases are ever renamed. This aligns with the codebase convention of using the `Constants` classes instead of magic strings.

### Positive: Deprecation schedule follows policy

The `[Obsolete]` annotation on `Legacy` is updated from "Scheduled for removal in Umbraco 18" to "Scheduled for removal in Umbraco 19". Since this PR targets v18 and the repository policy states obsoleted members must remain for at least one full major version (obsoleted in N, removed no earlier than N+2), marking it for removal in v19 would mean it's available throughout v18 and removed in v19. However, `Legacy` was originally obsoleted in v16/v17, not v18. By the original policy, removal in v18 was already valid (obsoleted in v16, v17 was the deprecation period, v18 eligible for removal). The decision to extend the timeline to v19 is a user-friendly choice that gives consumers more time but is not strictly required by the policy. This is a judgment call by the PR author and is reasonable.

### Minor: `_defaultEventTypes` comment or documentation

The `_defaultEventTypes` array serves a dual purpose: it defines which event types get individual telemetry counters AND which events are excluded from the "custom events" count. Adding a brief comment explaining this dual purpose would help future maintainers understand the impact of adding or removing entries.

---

## Overall Assessment

This is a clean, focused PR with clear intent. The default payload switch from `Legacy` to `Minimal` fulfills a previously documented plan (noted in the now-removed remarks). The telemetry improvement is a good code quality enhancement. The only actionable item is the stale `<remarks>` comment on `WebhookPayloadType.Minimal`.

**Verdict**: Approve with minor nit (stale doc comment on `Minimal`).
