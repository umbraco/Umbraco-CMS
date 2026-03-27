## PR Review

**Target:** `origin/main` · **Based on commit:** `94461926bb`

Adds user management directly within the User Group workspace — a sidebar panel lists group members, supports adding/removing users, and persists changes on save via the User Group and User API endpoints.

- **Modified public API:** `UmbUserGroupWorkspaceContext` — new `userUniques` observable, new `setUserUniques()` method, overridden `load()`, `submit()`, `resetState()`, `getHasUnpersistedChanges()`
- **Other changes:** New "Users" sidebar box rendered in the User Group details view; layout changed from single-column to two-column grid.

---

### Important

- **`user-group-workspace.context.ts:88`**: `take: 10000` is an unbounded fetch disguised as a bounded one. If a group has more than 10,000 users, the list silently truncates and subsequent saves will **remove** the users beyond the page size (they won't appear in `pending`, so they end up in `toRemove`). This is a data-loss risk. Either paginate until exhausted, use a dedicated "get users by group" endpoint if available, or at minimum detect when `items.length === take` and warn/disable editing.

- **`user-group-workspace.context.ts:113-119`**: The `submit()` override has identical `if`/`else` branches — both call `super.submit()` then `#persistUserChanges()`. The conditional and comments suggest different handling was intended for new vs. existing groups but never materialized. Collapse to a single path to avoid confusion:
  ```ts
  override async submit() {
      await super.submit();
      await this.#persistUserChanges();
  }
  ```

- **`user-group-workspace.context.ts:130-131`**: If `#addUsersToGroup` succeeds but `#removeUsersFromGroup` fails (or vice versa), the server state is now partially updated but `#persistedUserUniques` is not updated (line 145 only runs when *both* succeed). On the next save, the successfully-added users will be added *again* (no-op if the API is idempotent, error if not), and the failed removals will be retried (correct), but the user sees stale dirty-state. Consider updating `#persistedUserUniques` to reflect the partial success — add the successfully-added IDs and keep the failed-to-remove IDs.

- **`user-group-workspace.context.ts`**: No tests accompany this new functionality. The user-change detection, parallel add/remove logic, and partial-failure handling are all behavioral logic that would benefit from unit tests (at minimum for `getHasUserChanges`, `#persistUserChanges`).

### Suggestions

- **`user-group-workspace.context.ts:14`**: `UserService` is imported but only used for `getFilterUser`. Consider importing just what's needed if tree-shaking isn't guaranteed, or note that it's a generated barrel export and this is fine.

- **`user-group-workspace.context.ts:100`**: `getHasUserChanges()` — `jsonStringComparison` compares via `JSON.stringify`, which is order-sensitive. If the API returns user IDs in a different order than the client state (e.g., after a refresh), this will report a false positive dirty state. Consider sorting both arrays before comparing, or using a Set-based comparison.

- **`user-group-details-workspace-view.element.ts:280-283`**: The grid layout `grid-template-columns: 1fr 350px` is a fixed sidebar width with no responsive breakpoint. On narrow viewports or embedded panels this may cause horizontal overflow. Consider a `min-width` media query or `minmax()`.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions — in particular the `take: 10000` data-loss scenario and the partial-failure state handling.
