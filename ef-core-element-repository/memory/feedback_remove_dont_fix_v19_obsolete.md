---
name: remove-dont-fix-v19-obsolete
description: "When touching a member marked \"Scheduled for removal in Umbraco 19\", delete it instead of patching it if the fix is otherwise mechanical"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 3629769a-7eca-4d89-9fd7-e4c33337b0f8
  modified: 2026-08-17T06:54:43.918Z
---

If a change would otherwise require editing the body of a member already marked `[Obsolete("...Scheduled for removal in Umbraco 19.")]` (or earlier), delete the member instead of fixing it in place — but only when removal is genuinely easy (no real callers, or a single trivially-inlinable call site). If it has real, non-trivial callers, leave it fixed as normal.

**Why:** The `v18/feature/ef-core-document-repository` EF Core migration work has slipped and will not ship before Umbraco 19. Anything scheduled for removal "in Umbraco 19" will be at or past its removal date by release. Confirmed on this branch: `version.json` already reads `19.0.0-beta1`, and both a legacy `IAuditRepository`+`IAuditService` obsolete `ContentService` constructor overload were deleted outright rather than patched during the `AsyncPublishableContentServiceBase` work, once past their declared removal point — see [[feedback_this_branch_allows_breaking_changes]].

**How to apply:** Before editing any `[Obsolete]` member during this branch's work, check its scheduled-removal version against 19 (or earlier). If it matches and has no real callers, delete it outright rather than adjusting its implementation. Don't extend this to members scheduled for 20+ or with genuinely large calling code. When in doubt, check callers first and report back rather than guessing.
