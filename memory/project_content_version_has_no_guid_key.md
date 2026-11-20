---
name: Content version has no Guid key
description: Decision 2026-09-29 - umbracoContentVersion keeps only its int id; the Guid key column, its NPoco migration, and the Guid-keyed async repository overloads were removed
metadata:
  type: project
---

On 2026-09-29 the user and a colleague decided that content versions stay int-identified: a version is not an entity in its own right, so `umbracoContentVersion.key` was removed from both DTOs, the `AddContentVersionKeyColumn` NPoco migration and its test were deleted, `IAsyncContentRepository.GetVersionAsync(Guid)`/`DeleteVersionAsync(Guid)` are gone, and the `ReconcileDocumentRepositoryModel` EF Core migrations were regenerated (still no-op Up/Down) so both snapshots no longer carry the column.

**Why:** the key never pointed at anything callers could hold; every service path already used the int version id, so the column was schema and code with no consumer.

**How to apply:** do not reintroduce a Guid on `ContentVersionDto` (either flavour) or Guid-keyed version members on the async repository tier; `GetVersionAsync(int)` is the contract. The historical sections in [[project_document_repository_retirement_plan]] that describe adding `Key = Guid.NewGuid()` are superseded.

**Dev databases (2026-09-29):** SQLite dev databases created while the key existed carry a `NOT NULL` key column with no default, so content saves fail on them after the removal. The user chose to recreate dev databases rather than ship a cleanup plan step; do not propose one again. Pushed as `ab8281cec8c` + `7eb73200a8c`.
