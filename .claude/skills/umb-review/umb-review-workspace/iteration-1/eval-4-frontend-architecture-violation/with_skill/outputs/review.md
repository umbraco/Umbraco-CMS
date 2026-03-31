## PR Review

**Target:** `origin/main` · **Based on commit:** `c19e424cdd9`

Adds user management (add/remove users) to the user group workspace, with a new sidebar panel showing a user input picker and persistence logic in the workspace context.

- **Other changes:** User group workspace now loads, displays, and persists user membership directly. A new sidebar Users panel appears on the user group details view.

---

### Important

- **`user-group-workspace.context.ts:13-14`**: Direct import of `UserGroupService` and `UserService` from `@umbraco-cms/backoffice/external/backend-api` violates the established data flow architecture. The documented rule in `data-flow.md` is: "Never call generated API services directly from elements or contexts -- always go through data source -> repository." The `#loadUsers`, `#addUsersToGroup`, and `#removeUsersFromGroup` methods all call generated API services directly. There is already an existing `UmbUserSetGroupsServerDataSource` in `user/user/repository/sources/user-set-group.server.data-source.ts` and a `UmbUserCollectionServerDataSource` that wraps `UserService.getFilterUser`. These operations should be routed through proper data sources (and ideally repositories) to maintain the layered architecture. -> Create a data source (e.g., `UmbUserGroupMembersServerDataSource`) that encapsulates the `postUserGroupByIdUsers`, `deleteUserGroupByIdUsers`, and `getFilterUser` calls, and consume it from the workspace context via a repository or at minimum a data source instance.

- **`user-group-workspace.context.ts:57`**: `take: 10000` is a hardcoded upper bound that fetches all users in a single request. For installations with thousands of users this will cause slow load times, high memory usage, and potentially hit server-side response limits. No pagination is implemented. -> Use the collection/pagination pattern or at minimum load users on-demand. If a full list is required, paginate with reasonable page sizes and aggregate.

- **`user-group-workspace.context.ts:94-123`**: `#persistUserChanges` runs add and remove in parallel, but if one succeeds and the other fails, neither result is reflected in local state (`#persistedUserUniques` is only updated when both succeed). This means the UI will show stale data -- e.g., if adds succeeded but removes failed, the successfully added users won't be tracked as persisted, causing them to be re-added on the next save. -> Update `#persistedUserUniques` incrementally: add the successfully-added IDs and remove the successfully-removed IDs independently, regardless of the other operation's outcome.

### Suggestions

- **`user-group-workspace.context.ts:83-91`**: The `if (this.getIsNew())` and `else` branches in `submit()` are identical. The comment suggests different intent ("create group first, then add users" vs "existing groups") but the code does the same thing. -> Remove the conditional and keep a single code path, or implement the actually-different logic for new vs existing groups if there is a distinction to make.

- **`user-group-details-workspace-view.element.ts:11`**: The import `import type { UmbUserInputElement } from '../../../../user/components/user-input/user-input.element.js'` reaches into the internal file structure of the `user` sub-package. The existing pattern within the `user` parent package (e.g., `user-invite-modal.element.ts` importing from `user-group/components/`) shows this is tolerated, but it creates tight coupling to internal file paths. -> Consider importing from the `user` sub-package's public `index.ts` or `components/index.ts` barrel to be resilient to internal file reorganization.

- **`user-group-workspace.context.ts:69-71`**: `setUserUniques` has a JSDoc comment but `getHasUserChanges`, `#loadUsers`, `#persistUserChanges`, `#addUsersToGroup`, and `#removeUsersFromGroup` do not. Per coding preferences, public methods should be documented. -> Add JSDoc to the public methods (`getHasUserChanges`, `getHasUnpersistedChanges` override). Private methods are less critical but the complex ones (`#persistUserChanges`) would benefit from documentation.

---

## Request Changes

Critical and important issues must be addressed first.
