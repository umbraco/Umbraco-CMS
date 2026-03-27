### Code review
Found 2 issues:

1. Stale XML remark on `Minimal` enum value not updated (misleading documentation)
   The `Minimal` enum member still carries the remark "Expected to be the default option from Umbraco 17." The PR's own description acknowledges this change was supposed to happen in v17 but did not, and is now being done in v18. The remark was not updated to reflect that `Minimal` is the actual default from Umbraco 18, leaving misleading documentation in a v18 release.
   src/Umbraco.Core/Webhooks/WebhookPayloadType.cs (Minimal enum value, <remarks> block)

2. `[Obsolete]` removal version does not comply with the N+2 rule (confidence: 85)
   CLAUDE.md §5.4 states: "If obsoleted in version N, the earliest removal is version N+2." This PR targets `v18/dev` (version 18) and changes the scheduled removal from v18 to v19. If the re-dating of the removal target is considered a fresh obsolescence decision in v18, the rule requires "Scheduled for removal in Umbraco 20" (18+2), not Umbraco 19. The PR description explicitly states "until V19 (at the earliest)", but this contradicts the documented policy. The only defensible reading is that the original obsolescence happened in v16, the message is merely being corrected, but the diff updates it while in a v18 context where N+2 = 20.
   src/Umbraco.Core/Webhooks/WebhookPayloadType.cs:29 ([Obsolete("Scheduled for removal in Umbraco 19.")])
