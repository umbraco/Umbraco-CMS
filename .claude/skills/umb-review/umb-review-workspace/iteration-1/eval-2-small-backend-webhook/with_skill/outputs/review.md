## PR Review

**Target:** `origin/v18/dev` · **Based on commit:** `1a86c9f45c7`

Changes the default webhook payload type from `Legacy` to `Minimal` (planned since v16), extends the `Legacy` obsolete removal target from v18 to v19, and adds Element webhook events to the telemetry provider's default event list while replacing hardcoded strings with core constants.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`; `WebhookPayloadType.Legacy` `[Obsolete]` removal version changed from Umbraco 18 to Umbraco 19
- **Affected implementations (outside this PR):** `WebhookSettings.cs` references `Constants.Webhooks.DefaultPayloadType` for its default `PayloadType` -- all installations without explicit webhook payload configuration will switch from Legacy to Minimal payloads
- **Other changes:** Webhook telemetry now tracks Element events (ElementDelete, ElementPublish, ElementUnpublish) as default event types, meaning element-event webhooks are no longer counted as "custom events" in telemetry

---

### Suggestions

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:13`**: The `Minimal` enum member's remark says "Expected to be the default option from Umbraco 17" -- now that it IS the default, the remark is stale. Consider updating to e.g. "This is the default option from Umbraco 17."

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.
