---
date: 2025-12-13T06:23:36+00:00
researcher: Claude
git_commit: a1184533860623a1636620c78be6151490b0ea77
branch: refactor/ContentService
repository: Umbraco-CMS
topic: "ContentService Refactoring Design Review and Revision"
tags: [design-review, refactoring, contentservice, architecture]
status: complete
last_updated: 2025-12-13
last_updated_by: Claude
type: implementation_strategy
---

# Handoff: ContentService Refactoring Design Review

## Task(s)

| Task | Status |
|------|--------|
| Review design document for major issues | Completed |
| Identify architecture/codebase mismatches | Completed |
| Make decisions on 9 major issues | Completed |
| Revise design document with corrections | Completed |
| Commit updated design | Completed |

The session focused on critically reviewing `docs/plans/2025-12-04-contentservice-refactoring-design.md` and identifying major issues where the design conflicted with the actual codebase structure.

## Critical References

- `docs/plans/2025-12-04-contentservice-refactoring-design.md` - The revised design document (primary artifact)
- `src/Umbraco.Core/Services/ContentService.cs` - The 3,823-line god class being refactored
- `src/Umbraco.Core/CLAUDE.md` - Core architecture guide with patterns

## Recent Changes

- `docs/plans/2025-12-04-contentservice-refactoring-design.md` - Complete revision addressing 9 major issues

## Learnings

### Critical Architecture Discovery
The original design assumed implementations go in `Umbraco.Infrastructure`, but the actual codebase has:
- `ContentService.cs` in `Umbraco.Core/Services/` (not Infrastructure)
- `ContentEditingService.cs` in `Umbraco.Core/Services/`
- `ContentPublishingService.cs` in `Umbraco.Core/Services/`

### Existing Service Complexity
`ContentPublishingService` already has:
- Background task handling via `ILongRunningOperationService`
- Schedule management with `ContentScheduleCollection`
- `IUmbracoContextFactory` integration

`ContentEditingService` uses inheritance from `ContentEditingServiceWithSortingBase`, not delegation.

### Return Type Patterns
Three incompatible patterns exist in the codebase:
1. Legacy `OperationResult`/`PublishResult` (IContentService)
2. Modern `Attempt<TResult, TStatus>` (IContentEditingService)
3. Proposed new types (design) - **rejected in favor of reusing existing**

### Pagination Patterns
- Legacy: `pageIndex/pageSize` with `out long totalRecords`
- Modern: `PagedModel<T>` containing items and total
- Design should use `PagedModel<T>` for new services

## Artifacts

Primary artifact produced:
- `docs/plans/2025-12-04-contentservice-refactoring-design.md` - Revised design document

Key sections added/updated:
- Lines 22-36: Design Decisions Log (new section documenting all 8 decisions)
- Lines 39-123: Revised Architecture section with corrected layer structure
- Lines 203-253: Updated `IContentQueryService` with `PagedModel<T>` and complete method list
- Lines 255-338: Simplified pipeline components using existing types
- Lines 342-389: Corrected file locations (all in Umbraco.Core)

## Action Items & Next Steps

1. **Begin Phase 1 Implementation** - Introduce pipeline components:
   - Create `IPublishingValidator`, `IPublishingExecutor`, `IPublishingStrategy`, `IPublishingNotifier`
   - Location: `src/Umbraco.Core/Services/Publishing/`
   - Components only depend on repositories (one-way dependencies)

2. **Create Phase 1 Implementation Plan** - Run `/superpowers:write-plan` to generate detailed task breakdown for Phase 1

3. **Set Up Benchmarks** - Create `tests/Umbraco.Tests.Benchmarks/ContentPublishingBenchmarks.cs` to establish baseline before changes

4. **Integration with ContentPublishingService** - Modify `ContentPublishingService` to use new components internally while keeping public API unchanged

## Other Notes

### Key Design Decisions Made

| # | Issue | Decision |
|---|-------|----------|
| 1 | Architecture | Implementations in `Umbraco.Core` (match existing) |
| 2 | Service relationships | Gradual extraction - components are helpers |
| 3 | Dependencies | One-way only - components depend on repositories |
| 4 | Async/sync | Query service stays sync |
| 5 | Return types | Reuse existing `PublishResult` |
| 6 | Pagination | Use `PagedModel<T>` |
| 7 | DI registration | Add to existing `UmbracoBuilder.Services.cs` |
| 8 | Query capabilities | Pragmatic split |

### Removed from Original Design
- `IContentMutationService` - keep in facades
- `IContentTreeService` - keep in facades
- `IContentPublishingPipeline` - use individual components instead
- New status enums - reuse existing types

### Important File Locations
- Existing services: `src/Umbraco.Core/Services/`
- DI registration: `src/Umbraco.Infrastructure/DependencyInjection/UmbracoBuilder.Services.cs`
- Test location: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/`
