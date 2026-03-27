## PR Review

**Target:** `origin/v18/dev` (from argument) · **Based on commit:** `0538555e98f` · **Files:** 3 changed, 0 skipped, 3 reviewed (3 full, 0 diff + header-only)

Changes the default webhook payload type from `Legacy` to `Minimal`, extends the `Legacy` enum value's deprecation window by one version (from "removal in Umbraco 18" to "removal in Umbraco 19"), and updates the webhook telemetry provider to use Core constants and include Element webhook events in the default tracking list.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`; `WebhookPayloadType.Legacy` `[Obsolete]` attribute updated from "Umbraco 18" to "Umbraco 19"
- **Affected implementations (outside this PR):** `WebhookSettings.cs` (uses `Constants.Webhooks.DefaultPayloadType` for its default), `UmbracoBuilder.Collections.cs` (reads the constant at DI registration time), `WebhookEventCollectionBuilderExtensions.cs` (uses the constant as a default parameter value). All these will automatically pick up the new `Minimal` default without code changes. Additionally, ~30 sub-builder extension methods in `WebhookEventCollectionBuilderCms*Extensions.cs` still have `WebhookPayloadType.Legacy` as their parameter default, but these are only used when called directly by external consumers (the normal DI path always passes the payload type explicitly).
- **Other changes:** Webhook telemetry now tracks 3 additional default event types (ElementDelete, ElementPublish, ElementUnpublish) and uses `Constants.WebhookEvents.Aliases.*` instead of hardcoded strings, which will affect telemetry metrics reported for existing installations (previously, Element webhooks were counted as "custom events" rather than tracked individually).

---

### Important

- **`src/Umbraco.Core/Configuration/Models/WebhookSettings.cs:82-84`**: The XML doc comment on the `PayloadType` property still reads _"By default, Legacy payloads are used ... This default will change to minimal starting from v17."_ This is now stale because the default has been changed to `Minimal`. The remarks should be updated to reflect the current behavior (e.g., "By default, Minimal payloads are used.") to avoid confusing developers reading the configuration model.

---

### Suggestions

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:12-13`**: The `<remarks>` on `Minimal` still says _"Expected to be the default option from Umbraco 17."_ Since this PR targets v18/dev and `Minimal` is now the actual default, consider updating this to reflect that it became the default in Umbraco 17 (or 18), rather than using future tense.

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:28`**: The `[Obsolete("Scheduled for removal in Umbraco 19.")]` attribute targets version 19 for removal. Per the repository's obsolete policy, items obsoleted in version N should target N+2 for removal. Since `Legacy` was originally obsoleted around v16, the original target of v18 was correct (16+2=18). Extending to v19 is a deliberate choice documented in the PR description and gives users more time to migrate, so this is reasonable. Just noting it deviates from the standard formula -- no action needed.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.
