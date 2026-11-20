---
name: feedback-unqualified-cref-fails-across-generic-base
description: An unqualified <see cref> does not resolve to a member inherited through a generic base or interface; grep CS1574 after any rename
metadata: 
  node_type: memory
  type: feedback
  originSessionId: c61c9c25-746f-4318-b3b3-9e5cf5da0d87
  modified: 2026-09-17T08:53:41.299Z
---

`<see cref="SomeMember" />` written unqualified inside a class does **not** resolve when `SomeMember` is
inherited through a generic base class or interface. Qualify it:
`<see cref="IAsyncPublishableContentService{TContent}.UnpublishAsync" />`.

**Why:** retiring `PublishBranch`, a reviewer caught that a `<remarks>` block still named the old
`CommitDocumentChanges`/`CommitContentChangesInternal`. Fixing those revealed a *third* orphan
(`<see cref="Unpublish" />`) left by an earlier increment — and my replacement `<see cref="UnpublishAsync" />`
still emitted `CS1574`, because `UnpublishAsync` lives on `AsyncPublishableContentServiceBase<TContent>` /
`IAsyncPublishableContentService<TContent>`, not on `ContentService` itself.

**How to apply:** after any rename or member removal, rebuild with `--no-incremental` and grep the output for
`CS1574` scoped to the files you touched — nothing else catches an orphaned cref, and the renamer is the one
who owns it. Worth doing routinely in this campaign: removing an interface member also orphans `<inheritdoc/>`
on surviving implementations with no diagnostic at all
(see [[feedback_shared_interface_removal_breaks_by_receiver_type]]).
