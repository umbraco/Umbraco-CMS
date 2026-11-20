---
name: project_member_service_async_tier
description: IMemberService and IMediaService implement IAsyncContentServiceBase<T> by delegating to their sync members — merge reconciliations so the editing services can share the async base, not real migrations
metadata:
  type: project
---

On `v18/feature/ef-core-document-repository` (merge of 2026-09-22), `IMemberService` gained
`IAsyncContentServiceBase<IMember>`; `MemberService` implements `GetByIdAsync`, `SaveAsync` and
`CheckDataIntegrityAsync` by delegating to the existing synchronous members. Members are **not**
backed by an async repository — nothing about the data path changed.

It exists because two branches collided: `ef-core-repositories` moved `MemberContentEditingService`
onto `AsyncContentEditingServiceBase` (forced — `IMemberTypeService` had become async-only, and the
sync `ContentEditingServiceBase` requires `IContentTypeBaseService`), while the document-repository
branch tightened that base's `TContentService` constraint to `IAsyncContentServiceBase<TContent>`.
The user chose this over relaxing the constraint. The XML docs on both `IAsyncContentServiceBase`
interfaces were updated — they previously said members never implement it.

**Why:** the delegating implementations look like an unfinished migration and invite someone to
"finish" them. They are load-bearing only for the generic constraint.

**How to apply:** when members or media get an async repository, replace the delegating bodies rather
than adding a parallel API.

Update 2026-09-28: merging ef-core-repositories (EF Core media type repository, #23970) moved
`MediaEditingService` onto `AsyncContentEditingServiceWithSortingBase`, so `IMediaService` got the same
delegating tier (`GetByIdAsync`, `SaveAsync`, `CheckDataIntegrityAsync`) and the editing service resolves
`Guid? parentKey` to ids through the sync media service. No content service is outside the tier any more.
