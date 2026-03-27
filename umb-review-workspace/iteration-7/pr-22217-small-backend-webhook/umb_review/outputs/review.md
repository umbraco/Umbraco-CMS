## PR Review

**Target:** `origin/v18/dev` · **Based on commit:** `0538555e98f` · **Skipped:** 0 noise files out of 3 total

Changes the default webhook payload from `Legacy` to `Minimal`, defers removal of `Legacy` to v19 (from v18), and improves the telemetry provider by replacing hardcoded event alias strings with constants and adding element event types.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` (value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`); `WebhookPayloadType.Legacy` `[Obsolete]` message updated
- **Affected implementations (outside this PR):** `WebhookSettings.PayloadType` default (via `StaticPayloadType = Constants.Webhooks.DefaultPayloadType`); `WebhookEventCollectionBuilderExtensions.AddCms` default parameter; `UmbracoBuilder.Collections.cs` fallback value — all pick up the new default automatically
- **Other changes:** Any existing Umbraco installation upgrading to this version that has not explicitly configured a `PayloadType` will silently switch from `Legacy` to `Minimal` webhook payloads on upgrade. This is a deliberate runtime behavior change for unconfigured sites. Users relying on the old default (legacy full-model payloads) must explicitly set `Umbraco:CMS:Webhook:PayloadType: Legacy` in their config to retain previous behavior.

---

### Important

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:29`**: `[Obsolete("Scheduled for removal in Umbraco 19.")]` — wrong version. Per the project rule, removal version must be `current_major + 2`. `version.json` declares `18.0.0-beta1`, so current major is **18** and the correct target is **Umbraco 20**, not 19. Fix: `[Obsolete("Scheduled for removal in Umbraco 20.")]` and update the `<remarks>` block accordingly.

### Suggestions

- **`src/Umbraco.Core/Configuration/Models/WebhookSettings.cs:74–77`**: The `<remarks>` on `PayloadType` still reads _"By default, Legacy payloads are used… This default will change to minimal starting from v17."_ — now stale for two versions. Not part of this diff, but this PR is the natural moment to fix it (update to _"By default, Minimal payloads are used."_).

---

## Request Changes

Critical and important issues must be addressed first.
