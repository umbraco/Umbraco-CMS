---
name: contentbase-parentkey-returns-null-not-throws
description: ContentBase.ParentKey getter returns null (not throws) when the parent key was never populated; use TryGetParentKey to distinguish "unknown" from root
metadata:
  type: project
---

As of 2026-09-24, `ContentBase.ParentKey`'s getter is `TryGetParentKey(out key) ? key : null` and never throws. An older shape did throw; a stale comment in ContentService.MoveAsync still claims it does.

**Why:** a null ParentKey is ambiguous — it means root OR "never populated". Passing it straight into navigation/cache code silently attaches unknown-parent content to root instead of failing.

**How to apply:** compare `.ParentId` (int) when you only need equality; call `TryGetParentKey` when you need to know whether the key is actually known. Don't write comments claiming the getter throws. Related: [[root-content-has-no-guid-key]].
