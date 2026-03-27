### Code review

Found 1 issue:

1. Stale XML doc remark on `WebhookPayloadType.Minimal` (outdated documentation)
   `src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:12-13`

   The `Minimal` enum value still has the remark `"Expected to be the default option from Umbraco 17."` This is now stale: this PR changes the default to `Minimal` in v18, and the statement was already inaccurate since v17 shipped without the change. The remark should be updated (e.g., `"This is the default option from Umbraco 18."`) or removed entirely since the value is now simply the default. The PR correctly cleaned up the equivalent remark in `Constants-Webhooks.cs` but missed this one in the same file being modified.

---

**Other findings that did not meet the 80+ confidence threshold:**

- **`[Obsolete]` schedule change (Legacy)**: Changed from "Scheduled for removal in Umbraco 18" to "Scheduled for removal in Umbraco 19". This is more conservative than the minimum policy (which would allow removal at 18 since it was obsoleted in 16), so it is acceptable. The PR intentionally extends the deprecation period. (Confidence: 0 - not an issue)

- **Telemetry default event type changes**: String literals replaced with `Constants.WebhookEvents.Aliases.*` constants and Element event types added. This is a pure improvement. (Confidence: 0 - not an issue)
