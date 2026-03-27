## PR Review

**Target:** `origin/v18/dev` · **Based on commit:** `0538555e98f02ba24d5ad555368d47a0784dc868` · **Files:** 3 changed, 0 skipped, 3 reviewed

This PR changes the default webhook payload type from `Legacy` to `Minimal` (the announced intent since Umbraco 17), updates the `Legacy` enum value's obsolete removal target, replaces hardcoded event alias strings in telemetry with Core constants, and adds three missing element event types to the telemetry default event list.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` (value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`); `WebhookPayloadType.Legacy` obsolete attribute (removal version updated)
- **Affected implementations (outside this PR):** `WebhookSettings.StaticPayloadType` (references the const and therefore now defaults to `Minimal`); `WebhookEventCollectionBuilderExtensions.AddCms` (uses `Constants.Webhooks.DefaultPayloadType` as default param — value silently changes); `UmbracoBuilder.Collections.cs` (reads `Constants.Webhooks.DefaultPayloadType` at startup)
- **Other changes:** New Umbraco 18 installs (and upgrades where `PayloadType` is not explicitly configured) will use `Minimal` webhook payloads instead of `Legacy`. Telemetry now tracks three additional element events: `ElementDelete`, `ElementPublish`, `ElementUnpublish`.

---

### Critical

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:28`**: The `[Obsolete]` removal version is incorrect. The repo is at major version 18 (per `version.json`), so the removal target must be `current + 2 = 20`. The PR updated it from "18" to "19", but it should read "20".

  ```csharp
  // Correct:
  [Obsolete("Scheduled for removal in Umbraco 20.")]
  Legacy = 2,
  ```

  The XML doc `<remarks>` on the same member should also be updated to say "available as a configurable option for Umbraco 17, 18, and 19" for consistency with the new removal target.

### Important

- **`src/Umbraco.Core/Webhooks/WebhookEventCollectionBuilderCmsContentExtensions.cs:20` (and 6 other builder files)**: 65 internal usages of `WebhookPayloadType.Legacy` as default parameter values (e.g., `WebhookPayloadType payloadType = WebhookPayloadType.Legacy`) exist across `WebhookEventCollectionBuilderCmsContentExtensions.cs`, `WebhookEventCollectionBuilderCmsExtensions.cs`, `WebhookEventCollectionBuilderCmsMemberExtensions.cs`, `WebhookEventCollectionBuilderCmsUserExtensions.cs`, `WebhookEventCollectionBuilderCmsFileExtensions.cs`, and `WebhookEventCollectionBuilderCmsContentTypeExtensions.cs`. Because `WebhookPayloadType.Legacy` is now `[Obsolete]`, these internal usages will produce CS0618 compiler warnings. Per the CLAUDE.md convention, no internal code should reference obsolete members. Each affected method's default parameter should be updated from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal` (the new project default), with `#pragma warning disable/restore CS0618` added only where the `case WebhookPayloadType.Legacy:` switch branches must remain to handle explicitly configured legacy payloads.

- **`src/Umbraco.Core/Configuration/Models/WebhookSettings.cs:82-83`**: The XML documentation for the `PayloadType` property was not updated and is now stale:

  > "By default, Legacy payloads are used see `WebhookPayloadType` for more info. This default will change to minimal starting from v17."

  After this PR, the default IS Minimal and Umbraco 17 is in the past. The remarks should be updated to reflect the current reality (e.g., "By default, Minimal payloads are used."). This file was not included in the PR diff but is directly affected by the constant value change.

### Suggestions

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:12`**: The `<remarks>` on `Minimal` says "Expected to be the default option from Umbraco 17." Now that this PR makes it the actual default, the remark could be updated to reflect it IS the default from Umbraco 18 — but this is cosmetic.

- **`src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`**: No tests exist for `WebhookTelemetryProvider`. The newly added element event aliases (`ElementDelete`, `ElementPublish`, `ElementUnpublish`) expand what's tracked, and a unit test verifying that the correct set of event types is reported would be a good addition to prevent future regressions.

---

## Request Changes

Critical and important issues must be addressed first.
