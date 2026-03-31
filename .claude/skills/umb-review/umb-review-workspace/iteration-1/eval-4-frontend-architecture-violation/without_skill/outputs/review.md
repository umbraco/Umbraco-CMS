# Code Review: PR #22215 -- User Management in User Group Workspace

**PR**: #22215 (2-file frontend feature)
**Target**: `origin/main`
**Files Changed**: 2 (152 lines added)

---

## Summary

This PR adds the ability to manage users directly from the user group workspace. It extends the workspace context to load, track, and persist user-group membership changes, and adds a `<umb-user-input>` element to the user group details view.

---

## Issues

### 1. Direct API Calls from Workspace Context -- Architecture Violation

**Severity: HIGH**

**Files**:
- `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 54-61, 126-148 in the diff; methods `#loadUsers`, `#addUsersToGroup`, `#removeUsersFromGroup`)

The workspace context directly imports and calls `UserService` and `UserGroupService` from `@umbraco-cms/backoffice/external/backend-api`:

```typescript
import { UserGroupService, UserService } from '@umbraco-cms/backoffice/external/backend-api';
```

The documented data flow architecture (`docs/data-flow.md`) explicitly states:

> **Never call generated API services directly from elements or contexts** -- always go through data source -> repository

The established data flow chain is:

```
Element -> Context -> Repository -> Data Source -> Generated API Client -> Server
```

Every other usage of `UserService` and `UserGroupService` in this package follows this pattern -- they are only ever imported in `*.server.data-source.ts` files. The context should delegate to a data source (or repository) rather than making HTTP calls directly.

The proper fix is to create a data source (and optionally a repository) for user-group membership operations, similar to the existing `UmbUserSetGroupsServerDataSource` at `src/Umbraco.Web.UI.Client/src/packages/user/user/repository/sources/user-set-group.server.data-source.ts` which handles the inverse operation (setting groups for users).

---

### 2. Cross-Sub-Module Deep Relative Import (Value Import)

**Severity: HIGH**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/views/user-group-details-workspace-view.element.ts` (lines 11, 14 in the diff)

```typescript
import type { UmbUserInputElement } from '../../../../user/components/user-input/user-input.element.js';
import '../../../../user/components/user-input/user-input.element.js';
```

This imports a component from the `user` sub-module into the `user-group` sub-module using a deep relative path that reaches into another module's internal file structure. The architecture documentation (`docs/architecture.md`) states:

> Cross-package imports increase coupling -- minimize them. Use public `index.ts` exports, never import another package's internal files.

The `UmbUserInputElement` is properly exported from `@umbraco-cms/backoffice/user` (via the package.json exports field). While `user` and `user-group` are sub-modules within the same `user` package, they are still separate modules with their own `index.ts` public APIs. The import should use the public export path:

```typescript
import type { UmbUserInputElement } from '@umbraco-cms/backoffice/user';
import '@umbraco-cms/backoffice/user';  // or a more targeted side-effect import
```

Note: there is one existing precedent in the codebase for a cross-sub-module relative import (`user-invite-modal.element.ts` importing from `user-group`), but that is a **type-only** import (erased at build time). This PR's import includes a **value/side-effect** import, which creates a hard runtime coupling to the internal file path of another module.

---

### 3. Unbounded Query: `take: 10000`

**Severity: MEDIUM**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (line 55 in the diff)

```typescript
UserService.getFilterUser({ query: { userGroupIds: [unique], take: 10000 } }),
```

Fetching up to 10,000 users in a single request is problematic:
- It makes assumptions about the maximum number of users (large installations can exceed this).
- It loads all user data into memory at once with no pagination.
- The response payload could be very large, impacting performance.

If a full list is needed, consider implementing pagination within the data source layer, or document why a large upper bound is acceptable. At minimum, this magic number should be extracted into a named constant.

---

### 4. Redundant Branching in `submit()`

**Severity: LOW**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 83-92 in the diff)

```typescript
override async submit() {
    if (this.getIsNew()) {
        // For new groups: create group first (so it exists on server), then add users.
        await super.submit();
        await this.#persistUserChanges();
    } else {
        // For existing groups
        await super.submit();
        await this.#persistUserChanges();
    }
}
```

Both branches execute identical code. The `if/else` is dead logic. This should be simplified:

```typescript
override async submit() {
    await super.submit();
    await this.#persistUserChanges();
}
```

If the distinction is intentionally future-proofing, a comment explaining the intent would be needed, but as written it is purely redundant.

---

### 5. Partial Failure Leaves Inconsistent State

**Severity: MEDIUM**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 104-123 in the diff)

The `#persistUserChanges` method runs add and remove operations in parallel, but only updates the persisted state if both succeed:

```typescript
if (!addError && !removeError) {
    this.#persistedUserUniques = [...pending];
}
```

If adding succeeds but removing fails (or vice versa), the local `#persistedUserUniques` remains stale, which means:
- `getHasUserChanges()` will continue to report changes.
- On next save, the successfully-completed operation will be re-attempted (adding users that are already members, or removing users that are already removed).

The re-attempts are likely idempotent at the API level, so this may not cause errors, but it is wasteful and the local state becomes a lie. Consider updating `#persistedUserUniques` partially based on which operations succeeded.

---

### 6. Missing Error Handling for `super.submit()` Failure

**Severity: MEDIUM**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 83-92 in the diff)

If `super.submit()` throws (which it can -- the base class throws on missing data or unique), `#persistUserChanges()` should not execute. Currently, if the base `submit()` fails to create or update the user group, the code will still attempt to modify user membership on a potentially non-existent group. The base class throws exceptions rather than returning error tuples, so a `try/catch` or checking for the error state before proceeding is warranted.

---

### 7. Missing Spacing in Notification Data Object

**Severity: LOW (formatting)**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 111, 117 in the diff)

```typescript
data: {headline: 'An error occurred', message: 'Can not add users to the group.' },
```

Missing space after `{` before `headline`. Should be:

```typescript
data: { headline: 'An error occurred', message: 'Can not add users to the group.' },
```

---

### 8. Hardcoded UI Strings (Not Localized)

**Severity: MEDIUM**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/user-group-workspace.context.ts` (lines 111-118 in the diff)

The notification messages use hardcoded English strings:
- `'An error occurred'`
- `'Can not add users to the group.'`
- `'Can not remove users from the group.'`

Per the project conventions (`docs/package-development.md`):

> No hardcoded UI-facing strings. All user-visible text must go through the localization system.

These should use localization keys via `this.localize.term(...)` or by passing localization keys to the notification context.

---

### 9. Layout Change May Affect Existing UI

**Severity: LOW**

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/user-group/workspace/user-group/views/user-group-details-workspace-view.element.ts` (lines 228-230 in the diff)

```css
#main {
    display: grid;
    grid-template-columns: 1fr 350px;
    gap: var(--uui-size-layout-1);
    padding: var(--uui-size-layout-1);
}
```

The `#main` container is changed from a simple padded block to a two-column grid layout. This is a structural change that affects all content within `#main`, not just the new user list. The existing `<umb-stack>` content now occupies the first column while the new user box takes the second. This should be tested across different viewport sizes to ensure the existing access/permission panels do not break on narrow screens. There is no responsive handling (e.g., media queries to stack on small screens), and the fixed `350px` column will not shrink.

---

## Positive Aspects

- The `resetState()` override correctly cleans up the user-specific state alongside the parent state.
- Using `UmbArrayState` with `jsonStringComparison` for dirty checking is a sensible approach for tracking changes.
- The `getHasUnpersistedChanges()` override integrates user changes with the existing workspace dirty-state mechanism.
- The use of `<umb-user-input>` is the right component choice for this UI.

---

## Verdict: REQUEST CHANGES

The PR has two high-severity architecture violations that must be addressed before merging:

1. **Direct API calls from the workspace context** bypass the established repository/data-source pattern that every other entity operation in the codebase follows. This should be refactored into a proper data source (and optionally repository) layer.

2. **Deep relative import across sub-module boundaries** should use the public package export path (`@umbraco-cms/backoffice/user`) rather than reaching into another module's internal files.

Additionally, the hardcoded English strings should be localized, the unbounded `take: 10000` query needs consideration, and the error handling around `super.submit()` failure should be tightened.
