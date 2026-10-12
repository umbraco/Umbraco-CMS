---
name: async-repo-historical-version-property-data
description: "RESOLVED, not a defect: publishing via the repository without calling PublishCulture first leaves superseded versions with NO property rows, because the write path deletes existing rows the new set does not account for. PublishCulture is what gives a property a published value. Investigated 2026-09-18."
metadata: 
  node_type: memory
  type: project
  originSessionId: 149720ab-fe51-4ab2-b3fb-3ee646dbcc5c
  modified: 2026-09-18T09:37:00.483Z
---

**Not a bug.** Publishing a document through `AsyncDocumentRepository.SaveAsync` by setting
`PublishedState = PublishedState.Publishing` **without first calling
`content.PublishCulture(CultureImpact.Invariant, DateTime.UtcNow, propertyEditorCollection)`** leaves
every superseded version with **no `umbracoPropertyData` rows at all**. Add the `PublishCulture` call and
all historical versions retain their own names *and* property values, matching NPoco exactly.

## Why

`PublishCulture` is what gives a property its *published* value. In
`AsyncDocumentRepository.PersistUpdatedItemAsync` a publishing save freezes the current version row as
the published one and inserts a new draft pair, then calls
`PersistUpdatedPropertyDataAsync(db, item, versionToDelete: item.PublishedVersionId, publishedVersionId)`.
That method loads existing rows for the frozen version, builds the new set via
`PropertyFactory.BuildEFCoreDtos(..., item.VersionId, publishedVersionId, ...)`, and **deletes every
existing row the new set does not account for**. With no published value, no rows are emitted for the
frozen version, so its existing rows are the unmatched ones and get deleted.

## Evidence

Same loop, the only difference being the `PublishCulture` call:

```
without PublishCulture          with PublishCulture
v11 name-4 title=title-4        v11 name-4 title=title-4
v10 name-4 title=               v10 name-4 title=title-4
v9  name-3 title=               v9  name-3 title=title-3
v8  name-2 title=               v8  name-2 title=title-2
v7  name-1 title=               v7  name-1 title=title-1
```

A direct `db.PropertyData` query confirmed the rows are genuinely absent in the first case — a write-side
effect, not a read/assembly one.

## Confirmed against the real service layer (2026-09-21)

Publishing three times through `IContentService` (all returning `SuccessPublish`) leaves **every** frozen
version holding its own property values:

```
version=10 current=True  text=svc-3 propvalues=[svctitle-3, ...]
version=9  current=False text=svc-3 propvalues=[svctitle-3, ...]
version=8  current=False text=svc-2 propvalues=[svctitle-2, ...]
version=7  current=False text=svc-1 propvalues=[svctitle-1, ...]
```

So the populated frozen version is what the system genuinely produces, and the tests were the outlier -
adding `PublishCulture` is not writing unused data. The production paths call it themselves:
`ContentService.cs:487`/`:492` and `AsyncPublishableContentServiceBase.cs:775/843/897/1170/1984`.

**Trap hit while checking this**: the first probe showed only ONE version row, because the content was
parented under an unpublished node and all three publishes were failing **silently**. Always assert on
the publish result (`SuccessPublish`) before drawing conclusions from a publish-based probe - the same
trap that bit the Rollback work earlier in this campaign.

## Fixed (commit `54004b8e987`)

All 9 publishing sites in `AsyncDocumentRepositoryTest` that lacked `PublishCulture` now have it. Every
test still passed unchanged - which was itself the finding: they all assert on version-row mechanics or
tag relationships, never on published property values, so none could have noticed. The publish test now
also asserts the published version keeps the value it was published with; removing `PublishCulture` from
it fails that assertion with `null`.

## Related

`GetVersionAsync_ForAnOlderVersion_ReturnsThatVersionsNameAndPropertyValues` in
`AsyncDocumentRepositoryTest` is the ported test; its `<remarks>` records the `PublishCulture`
requirement so the trap is not rediscovered. See [[document-repository-retirement-plan]].
