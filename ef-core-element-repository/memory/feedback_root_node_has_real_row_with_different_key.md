---
name: root-node-has-real-row-with-different-key
description: "umbracoNode id -1 (Root) is a REAL seeded row with UniqueId = Constants.System.RootSystemKey - never assume a self-join or lookup against umbracoNode returns null/no-row for Root; it returns a real, wrong Guid unless explicitly special-cased"
metadata:
  type: feedback
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-08-20T10:39:01.936Z
---

While replacing `IContentBase.ParentKey`'s batched-follow-up-query population with a self-join directly in
each repository's base query (joining `umbracoNode` to itself via `ParentId = NodeId`), a naive
implementation leaked `Constants.System.RootSystemKey` (`916724a5-173d-4619-b97e-b9de133dd6f5`) as the
`ParentKey` for every top-level (directly-under-root) Document, Media, and Member - instead of the
contractually-required `null` (`Constants.System.RootKey = null`).

**Why:** `Constants.System.Root` (`-1`) is commonly treated as a pure sentinel with no backing data, but
`DatabaseDataCreator.cs` actually seeds a REAL `umbracoNode` row for it (`NodeId = -1`,
`UniqueId = Constants.System.RootSystemKey`, `NodeObjectType = Constants.ObjectTypes.SystemRoot`) - unlike
what you might assume from `Constants.System.RootKey` being statically `null`. The three RecycleBin
pseudo-parents (`-20`/`-21`/`-22`) are ALSO real seeded rows, but their `UniqueId` legitimately matches
their own sentinel constants (`RecycleBinContentKey` etc.), so joins resolve them correctly with no special
handling needed. Root is the one asymmetric case: real row, but the row's own key is NOT the value the
`ParentKey` contract expects for it.

This was caught by a **targeted revert TDD check** (temporarily removing just the `ParentId ==
Constants.System.Root ? null : ...` ternary while leaving the rest of the self-join in place) — a
broader "does the value look right" test alone was not enough; the discriminating check had to isolate
exactly this line. The leaked value was recognizable on sight as `RootSystemKey` because the test asserted
`Is.Null` and printed the actual Guid on failure.

**How to apply:** Any code resolving a Guid from `umbracoNode` by joining/looking up on `ParentId`,
`NodeId`, or similar for a potentially-pseudo parent (`Constants.System.Root`,
`RecycleBinContent/-Media/-Element`) must explicitly special-case `Root` to produce `null` (or whatever the
semantic "no parent" value is for that field) - never assume a missing/sentinel id means "no row found."
Verify by testing the Root case specifically with a revert that isolates just that guard, not just an
end-to-end pass. See also [[project_parentkey_feature_status]] for the broader ParentKey effort this was
found during.
