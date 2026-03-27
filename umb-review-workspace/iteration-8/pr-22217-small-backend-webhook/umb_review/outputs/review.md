## PR Review

**Target:** `origin/v18/dev` · **Based on commit:** `0538555e98f02ba24d5ad555368d47a0784dc868` · **Skipped:** 0 files out of 3 total

Changes `Constants.Webhooks.DefaultPayloadType` from `Legacy` to `Minimal` for Umbraco 18, updates the `[Obsolete]` removal target on `WebhookPayloadType.Legacy`, and extends the telemetry provider's default event list with Element webhook aliases.

- **Modified public API:** `Constants.Webhooks.DefaultPayloadType` (value changed from `WebhookPayloadType.Legacy` to `WebhookPayloadType.Minimal`); `WebhookPayloadType.Legacy` obsolete message (removal version updated from 18 to 19)
- **Affected implementations (outside this PR):** `WebhookSettings.StaticPayloadType` (derives its default from `DefaultPayloadType` — now resolves to `Minimal`); `WebhookEventCollectionBuilderExtensions.AddCms` default parameter (also uses `Constants.Webhooks.DefaultPayloadType` as a compile-time default — callers omitting the argument will silently switch payload type); `UmbracoBuilder.Collections.cs` uses `DefaultPayloadType` as its fallback when config section is absent.
- **Other changes:** Any existing Umbraco 18 installation that does not explicitly configure `Umbraco:CMS:Webhook:PayloadType` will have its webhook payload changed from `Legacy` to `Minimal` after upgrading. This is a runtime behavior change for consumers relying on the default.

---

### Important

- **`src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:30`**: The `[Obsolete]` message is updated to `"Scheduled for removal in Umbraco 19."`, but per CLAUDE.md the removal target must be `current_major + 2`. The current major version on `v18/dev` is 18 (from `version.json`), so the correct target is Umbraco 20, not 19. `Legacy` was first obsoleted with a v18 target, implying it was marked obsolete in v16. Re-obsoleting in v18 should set the new target to `18 + 2 = 20`. Fix: change both the `[Obsolete]` attribute and the `<remarks>` comment to say "Umbraco 20".

- **`src/Umbraco.Core/Configuration/Models/WebhookSettings.cs`** (not in diff — pre-existing, but made incorrect by this PR): The `<remarks>` on `PayloadType` still reads *"By default, Legacy payloads are used… This default will change to minimal starting from v17."* This comment is now factually wrong since `DefaultPayloadType` resolves to `Minimal`. While pre-existing, this PR's change is what renders it incorrect. Consider including a docs fix for this comment in the PR.

---

## Request Changes

Critical and important issues must be addressed first.
