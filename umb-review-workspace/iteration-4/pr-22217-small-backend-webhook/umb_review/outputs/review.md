## PR Review

**Target:** `origin/v18/dev` · **Based on commit:** `0538555e98f02ba24d5ad555368d47a0784dc868` · **Files:** 3 changed, 0 skipped, 3 reviewed

This PR changes the default webhook payload type from `Legacy` to `Minimal` for Umbraco 18, extends the `[Obsolete]` removal target for `Legacy` from v18 to v19, and expands the telemetry provider's default event type list to include element events while replacing hard-coded string literals with typed constants.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` (value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`); `WebhookPayloadType.Legacy` obsolete removal version updated from Umbraco 18 to Umbraco 19
- **Affected implementations (outside this PR):** `WebhookSettings.StaticPayloadType` (derives its default from `Constants.Webhooks.DefaultPayloadType`); `WebhookEventCollectionBuilderExtensions.AddCms` (default parameter uses `Constants.Webhooks.DefaultPayloadType`); `UmbracoBuilder.Collections.cs` (reads `Constants.Webhooks.DefaultPayloadType` as fallback when config section is absent)
- **Other changes:** New Umbraco 18 installations will default to the `Minimal` webhook payload type instead of `Legacy`. Existing installations that have not set `Umbraco:CMS:Webhook:PayloadType` explicitly in their configuration will silently switch to `Minimal` payloads on upgrade. The telemetry report gains three new per-event-type counters (`Umbraco.ElementDelete`, `Umbraco.ElementPublish`, `Umbraco.ElementUnpublish`), and webhooks registered for those event types will now be excluded from the "custom event" counter rather than counted as custom.

---

### Important

- **`src/Umbraco.Core/Configuration/Models/WebhookSettings.cs:82-84`**: The XML doc remarks on `PayloadType` still state "By default, Legacy payloads are used see `WebhookPayloadType` for more info. This default will change to minimal starting from v17." This is now actively wrong: the default is `Minimal` in v18. Any developer or tooling reading this doc comment will receive incorrect information. → Update the remarks to reflect that `Minimal` is the default as of Umbraco 18, for example: "By default, Minimal payloads are used from Umbraco 18 onwards. See `WebhookPayloadType` for available options."

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:12-14`**: The `Minimal` enum member's remarks read "Expected to be the default option from Umbraco 17." This is future tense but is now current as of v18. The PR updated the `Legacy` remarks correctly but missed `Minimal`. → Change to "Default option from Umbraco 17 onwards." or similar present/factual phrasing.

- **`src/Umbraco.Infrastructure/Telemetry/Providers/WebhookTelemetryProvider.cs`**: No tests were added or modified alongside the change to `_defaultEventTypes`. The three new element aliases now affect what is classified as a "custom" event for telemetry purposes — webhooks registered for element events that were previously counted under `WebhookCustomEvent` will now be counted under per-event counters. This behavioral change to the telemetry output has no coverage. → Add or update tests for `WebhookTelemetryProvider.GetInformation()` to assert the counts for element event types and verify that element events are excluded from the custom event counter.

### Suggestions

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs`**: The `[Obsolete]` attribute on `Legacy` was updated from "Scheduled for removal in Umbraco 18." to "Scheduled for removal in Umbraco 19." The current major version on the `v18/dev` branch is 18 (from `version.json`), so the correct removal target per the CLAUDE.md policy (current + 2) would be Umbraco 20. If the intent is to retain `Legacy` through v18 and remove it in v19 (i.e., this was obsoleted in v17), that is consistent with the one-full-major-version rule from v17. Confirm that the obsolete was originally placed in v17 and that v19 is the intended removal target — if so, the value is correct as written.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.
