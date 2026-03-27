# Code Review: PR #22217 — Change the default webhook payload type to "minimal"

**Target branch**: `v18/dev`
**PR author**: kjac (kja@umbraco.dk)
**Files changed**: 3
**Commits**: 2

---

## Summary of Changes

The PR makes three distinct changes:

1. **`src/Umbraco.Core/Constants-Webhooks.cs`** — Changes `DefaultPayloadType` from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`, and removes the now-stale `<remarks>` XML doc block that explained the transition timeline.

2. **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`** — Updates the `[Obsolete]` attribute on `Legacy` from "Scheduled for removal in Umbraco 18" to "Scheduled for removal in Umbraco 19", and updates the `<remarks>` doc accordingly to reflect availability in v17 and v18.

3. **`src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`** — Replaces five hardcoded string literals in `_defaultEventTypes` with the corresponding `Constants.WebhookEvents.Aliases.*` constants, and adds three new Element webhook event types (`ElementDelete`, `ElementPublish`, `ElementUnpublish`) to the set of default events tracked for telemetry.

---

## Breaking Change Assessment

### `DefaultPayloadType`: Behavioural breaking change for existing installations

This is the most significant concern in the PR. Changing `DefaultPayloadType` from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal` is a **behavioural breaking change** for any existing installation that relies on the default.

**How the default is consumed:**

- `WebhookSettings.StaticPayloadType` is initialised from `Constants.Webhooks.DefaultPayloadType`. This value becomes the C# default for the `PayloadType` property, meaning any installation that has NOT explicitly set `Umbraco:Webhook:PayloadType` in their `appsettings.json` will silently move from `Legacy` to `Minimal` on upgrade.
- `UmbracoBuilder.Collections.cs` reads from configuration at startup, and falls back to `Constants.Webhooks.DefaultPayloadType` when no configuration section is present (the same shift).
- `WebhookEventCollectionBuilderExtensions.AddCms` uses `Constants.Webhooks.DefaultPayloadType` as its default parameter value. Because `const` values are baked in at compile time for callers outside the assembly, any third-party packages that call `AddCms()` without explicitly supplying `payloadType` will continue using the baked-in `Legacy` value from whatever Umbraco version they were compiled against. This discrepancy in behaviour between in-process and out-of-process consumers may cause confusion and is worth documenting explicitly.

**Impact for upgraders:**

Any site upgrading from v17 with webhooks configured using the `Legacy` payload (the previous default) will have its webhook payloads silently change format without any migration notice, configuration warning, or startup log entry. Webhook consumers (external systems receiving the payloads) would receive differently shaped JSON without being informed.

**Recommendation:** The PR should include either:
- A startup warning log if the payload type was not explicitly configured but is now defaulting to `Minimal`, or
- Clear release-notes documentation (not strictly a code issue, but worth flagging), or
- A migration step that detects and explicitly writes the previous default to the configuration for upgrading installations.

### `[Obsolete]` extension on `Legacy`

The previous `[Obsolete]` message read "Scheduled for removal in Umbraco 18." The PR changes this to "Scheduled for removal in Umbraco 19."

Per the repository's breaking-change policy (CLAUDE.md section 5.4): "If obsoleted in version N, the earliest removal is version N+2." The `Legacy` member was obsoleted in v16 (as the default option for v16 and available in v17/18). Removal at v19 is therefore one version early if it was first marked obsolete in v17 — or correct if it was first marked obsolete in v16 (N=16, earliest removal=18, but given extended support through v18, removal at v19 is generous and consistent with the policy). This appears intentional and is aligned with the updated remarks.

There is no binary breaking change here; the enum member value `Legacy = 2` is retained and the attribute is non-destructive.

---

## Code Quality

### Hardcoded strings replaced with constants (good)

The replacement of five hardcoded `"Umbraco.ContentDelete"` etc. string literals with `Constants.WebhookEvents.Aliases.*` references is a clean improvement. It eliminates the risk of typos and ensures the telemetry provider stays in sync if alias values ever change.

The previous strings were correct (they match the constant values), so this is a non-functional refactor with positive maintainability value.

### New Element event types added to telemetry

Adding `ElementDelete`, `ElementPublish`, and `ElementUnpublish` to `_defaultEventTypes` in `WebhookTelemetryProvider` is appropriate given that v18 introduces element webhook support (Element constants are present in `Constants-WebhookEvents.cs` in both v18/dev and the PR). This ensures telemetry data about element webhook usage is collected alongside content and media events.

One minor observation: The telemetry comment block in `GetInformation()` still only mentions "content and media" events implicitly (it does not reference element events). The XML doc block summarising the metrics is not wrong, but it may be slightly misleading over time as the list grows. This is very minor.

### Inconsistency between `_defaultEventTypes` and what Umbraco ships as "defaults"

The `_defaultEventTypes` array is used to classify webhooks as "using a default event" vs. "using a custom event" in the telemetry output. Care should be taken to ensure this list stays aligned with whatever event types are registered by default in `UmbracoBuilder.Collections.cs` → `builder.WebhookEvents().AddCms(true, webhookPayloadType)`. Currently `ElementDelete/Publish/Unpublish` may or may not be in the default set depending on whether element webhook events are registered by default. If they are not registered as defaults in v18, including them in `_defaultEventTypes` would misclassify them (element webhooks configured by users would appear as "default" rather than "custom"). This should be verified.

---

## Stale Documentation

### `WebhookSettings.cs` — Remarks not updated

The `PayloadType` property in `WebhookSettings` still has:
```
By default, Legacy payloads are used see <see cref="WebhookPayloadType"/> for more info.
This default will change to minimal starting from v17.
```

This remark is now doubly stale: Legacy is no longer the default (it is `Minimal` as of v17/v18), and the "will change" language implies a future change that has already happened. This file was not touched in the PR. It should be updated to reflect reality.

### `WebhookPayloadType.Minimal` remarks not updated

The `Minimal = 0` enum member still reads:
```
Expected to be the default option from Umbraco 17.
```

Now that the PR makes this the actual default in v18 (and was intended to be default from v17), this remark could be updated to say "Default option from Umbraco 17." (present tense, confirmed fact). Minor, but degrades the quality of the generated API documentation.

---

## Architectural Issues

### `const` default parameter and baked-in values

`WebhookEventCollectionBuilderExtensions.AddCms` uses `Constants.Webhooks.DefaultPayloadType` as a default parameter. Because `DefaultPayloadType` is `const`, its value is baked into call sites at compile time. After the change:
- Code compiled against v17 (or earlier) packages that calls `AddCms()` without the `payloadType` argument will use the old baked-in `Legacy` value.
- Code compiled against v18 packages will use `Minimal`.

This is a subtle ABI concern for package developers who call `AddCms()` in their own composers. It is not a bug in the PR itself, but it demonstrates that using a `const` here creates a non-obvious versioning surface. Changing to a `static readonly` field would propagate the change dynamically but would be its own breaking change to the signature. This is an existing design issue, not introduced by this PR, but worth noting.

---

## Summary of Issues

| Severity | Issue |
|----------|-------|
| Medium | Silent payload format change for upgraders without explicit configuration — no upgrade path or warning |
| Low | `WebhookSettings.PayloadType` `<remarks>` doc is stale and misleading |
| Low | `WebhookPayloadType.Minimal` `<remarks>` uses future tense for something already done |
| Low | `_defaultEventTypes` in telemetry provider should be verified to match the actual set of "default" registered events |
| Info | `const` default parameter in `AddCms` bakes in the value for third-party callers — existing design limitation |

---

## Verdict

The core intent — changing the default to `Minimal` for v18 — is correct and well-timed. The `[Obsolete]` extension for `Legacy` to v19 is appropriately conservative. The telemetry refactor (constants + element events) is clean.

The main concern is the lack of a graceful upgrade path for existing sites that were implicitly using `Legacy` as their payload default. Without explicit configuration, upgrading to v18 will silently change the shape of all outgoing webhook payloads. A startup log warning or a migration note would substantially reduce the risk of breakage for existing integrations.
