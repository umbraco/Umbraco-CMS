---
date: 2025-12-20T00:38:15+00:00
researcher: Claude
git_commit: f4a01ed50d5048da7839cf1149177fc011a50c6c
branch: refactor/ContentService
repository: Umbraco-CMS
topic: "ContentService Refactoring Performance Review"
tags: [implementation, performance, refactoring, contentservice]
status: completed
last_updated: 2025-12-20
last_updated_by: Claude
type: implementation_strategy
---

# Handoff: ContentService Performance Review for Refactoring Design

## Task(s)

**Brainstorming session to review the ContentService refactoring design for performance improvements across four areas:**

| Area | Status | Decision |
|------|--------|----------|
| 1. Database Query Efficiency | **Completed** | Batch lookups (4 steps) |
| 2. Memory Allocation Patterns | **Completed** | Aggressive, incremental (7 steps) |
| 3. Concurrency & Locking | **Completed** | Moderate (4 steps) |
| 4. Caching Strategies | **Completed** | Moderate (4 steps) |

Working from the design document at `docs/plans/2025-12-19-contentservice-refactor-design.md`.

## Critical References

1. `docs/plans/2025-12-19-contentservice-refactor-design.md` - The refactoring design being reviewed
2. `src/Umbraco.Core/Services/ContentService.cs` - Current ~3800 line implementation being refactored
3. `src/Umbraco.Core/Services/IContentService.cs` - Interface contract

## Recent changes

No code changes made. This is a design review/brainstorming session.

## Learnings

### Database Query Efficiency Issues Identified

1. **N+1 Query in `GetContentSchedulesByIds`** (ContentService.cs:1025-1049): Loop calling `_idKeyMap.GetIdForKey` for each key individually instead of batch lookup.

2. **Multiple repository calls in `CommitDocumentChangesInternal`** (ContentService.cs:1461+): Separate calls for `GetContentSchedule`, `Save`, and `PersistContentSchedule` within same scope.

3. **Repeated `GetById` calls**: `IsPathPublishable` (line 1070), `GetAncestors` (line 792) make multiple single-item lookups.

**Decision Made**: User approved **Option 1 (Batch lookups)** approach:
- Add batch methods like `GetSchedulesByContentIds(int[] ids)`, `ArePathsPublished(int[] contentIds)`, `GetParents(int[] contentIds)`
- Fix N+1 in key-to-id resolution by adding/using `IIdKeyMap.GetIdsForKeys(Guid[] keys, UmbracoObjectTypes type)`

### Memory Allocation Issues Identified

1. **Excessive ToArray/ToList materializations**: Lines 1170, 814, 2650 - materialize collections just to iterate
2. **String allocations in hot paths**: Lines 1201, 2596-2598 - string concat in loops/move operations
3. **Lambda/closure allocations**: Line 1125-1127 - creates list and closure on every save
4. **Dictionary recreations**: Lines 555-556 - creates dictionary then iterates again

**Pending Decision**: User needs to choose approach:
- **Conservative**: Fix obvious issues (ToArray before iteration, string concat in loops)
- **Moderate**: Add StringBuilder pooling, avoid unnecessary materializations
- **Aggressive**: Full Span/ArrayPool usage, pooled collections

## Artifacts

- `docs/plans/2025-12-19-contentservice-refactor-design.md` - Design document updated to Revision 1.2 with Performance Optimizations section

## Action Items & Next Steps

All performance review tasks completed. Design document updated with:

1. ✅ Database Query Efficiency - 4 batch lookup improvements
2. ✅ Memory Allocation Patterns - 7 incremental optimization steps
3. ✅ Concurrency & Locking - 4 lock optimization steps
4. ✅ Caching Strategies - 4 cache optimization steps
5. ✅ Design document updated at `docs/plans/2025-12-19-contentservice-refactor-design.md` (Revision 1.2)

**Next step**: Proceed to implementation planning with `superpowers:writing-plans`

## Other Notes

### Architecture Context

The refactoring splits ContentService into 5 public services + 2 internal managers:
- `IContentCrudService` (~400 lines) - Create, Get, Save, Delete
- `IContentPublishOperationService` (~1000 lines) - Publish, Unpublish, Scheduling
- `IContentMoveService` (~350 lines) - Move, RecycleBin, Copy, Sort
- `IContentQueryService` (~250 lines) - Count, Paged queries, Hierarchy
- `IContentVersionService` (~200 lines) - Versions, Rollback
- `ContentPermissionManager` (internal) - Permissions
- `ContentBlueprintManager` (internal) - Blueprints

### Key Design Decisions Already Made

1. **Naming**: `IContentPublishOperationService` (not `IContentPublishingService`) to avoid collision with existing API-layer service
2. **Scope Pattern**: Caller-Creates-Scope (Ambient Scope) - nested scopes participate in parent transaction
3. **Lock Coordination**: Acquire locks at highest level that creates scope
4. **Dependency Direction**: Unidirectional - Publish/Move may call CRUD, no reverse dependencies

### Relevant Codebase Locations

- Core interfaces: `src/Umbraco.Core/Services/`
- Repository interfaces: `src/Umbraco.Core/Persistence/Repositories/`
- Scoping: `src/Umbraco.Core/Scoping/`
- ID/Key mapping: `IIdKeyMap` interface for key-to-id resolution
