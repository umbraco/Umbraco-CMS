## PR Review

**Target:** `origin/main` · **Based on commit:** `c19e424cdd9` · **Skipped:** 2 files out of 29 total

Adds a current user workspace modal allowing non-admin users to update their profile (language preference) and avatar, backed by new Management API endpoints and a new `SectionUserNoPermission` condition.

- **Modified public API:** `IUserService` (new `UpdateCurrentUserAsync`), `IUserPresentationFactory` (new `CreateUpdateCurrentUserModelAsync`)
- **Affected implementations (outside this PR):** Any external implementors of `IUserService` or `IUserPresentationFactory` will get compile errors
- **Breaking changes:** Both `IUserService.UpdateCurrentUserAsync` and `IUserPresentationFactory.CreateUpdateCurrentUserModelAsync` are added to public interfaces without default implementations (Pattern 3 violation)

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Layer spread:** 3 layers touched (Core, API, Frontend) across 27 reviewable files. Consider splitting by layer -- e.g., Core + API endpoints first, then Frontend consumers.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Critical

- **`src/Umbraco.Core/Services/IUserService.cs:95`**: `UpdateCurrentUserAsync` added to public interface `IUserService` without a default implementation. External consumers implementing this interface will get a compile error. --> Add a default implementation per Pattern 3 from CLAUDE.md, e.g.:
  ```csharp
  // TODO (V19): Remove the default implementation.
  Task<Attempt<IUser?, UserOperationStatus>> UpdateCurrentUserAsync(CurrentUserUpdateModel model)
      => Task.FromResult(Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.Unknown, null));
  ```

- **`src/Umbraco.Cms.Api.Management/Factories/IUserPresentationFactory.cs:37`**: `CreateUpdateCurrentUserModelAsync` added to public interface without a default implementation. Same Pattern 3 violation. --> Add a default implementation that throws or returns a default value.

### Important

- **`src/Umbraco.Core/Services/UserService.cs:288`**: `UpdateCurrentUserAsync` does not validate `LanguageIsoCode`. The sibling `UpdateAsync` (line 931) calls `ValidateUserUpdateModel` which checks `_isoCodeValidator.IsValid(model.LanguageIsoCode)` and returns `UserOperationStatus.InvalidIsoCode` on failure. The new method bypasses this, allowing invalid ISO codes to be persisted. --> Add ISO code validation before saving.

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts:17`**: `#abortController = new AbortController()` is a single shared instance. Once `abort()` is called on it, its signal stays aborted permanently, causing all subsequent `uploadAvatar` calls to fail immediately. --> Create a new `AbortController` per upload inside the `uploadAvatar` method instead of sharing one at the class level.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/core/section/conditions/section-user-no-permission/`**: No test file for `UmbSectionUserNoPermissionCondition`. The sibling `UmbSectionUserPermissionCondition` has `section-user-permission.condition.test.ts` covering both permitted and denied cases. The negated condition should have equivalent coverage.

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts:111`**: `URL.createObjectURL(file)` is called but never revoked via `URL.revokeObjectURL()`. This creates a small memory leak per avatar upload. Consider revoking the old URL when a new avatar is uploaded or when the component disconnects.

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-profile-settings.element.ts:42`**: Public `save()` method lacks JSDoc. This method is called externally by the parent modal element and is part of the component's public contract.

---

## Request Changes

Critical and important issues must be addressed first.
