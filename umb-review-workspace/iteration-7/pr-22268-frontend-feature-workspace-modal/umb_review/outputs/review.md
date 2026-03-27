## PR Review

**Target:** `origin/main` · **Based on commit:** `b14f852637cd24c401a66f0361bcc5fbc6f4f731` · **Skipped:** 2 files out of 29 total (`sdk.gen.ts`, `types.gen.ts`)

Adds a "current user workspace" modal accessible to users without Users section access, allowing them to update their language preference and avatar. Introduces new backend endpoints (`PUT /current/profile`, `DELETE /current/avatar`) and a frontend sidebar modal with the corresponding UI.

- **Modified public API:** `IUserService` (new method `UpdateCurrentUserAsync`); `IUserPresentationFactory` (new method `CreateUpdateCurrentUserModelAsync`)
- **Affected implementations (outside this PR):** Any external code implementing `IUserService` or `IUserPresentationFactory` will fail to compile when upgrading.
- **Other changes:** New condition `Umb.Condition.SectionUserNoPermission` (inverse of the existing section-user-permission condition) is added to the public section API; new modal token `UMB_CURRENT_USER_WORKSPACE_MODAL` added to the public current-user API.

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Layer spread:** 3 distinct production layers touched (Core, API, Frontend) with 27 reviewable files. Consider a first PR adding Core contracts and API implementations, followed by a second PR for the Frontend UI.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Critical

- **`src/Umbraco.Core/Services/IUserService.cs:100`**: `UpdateCurrentUserAsync` added to a public interface without a default implementation. Any plugin/package that implements `IUserService` will fail to compile. Apply Pattern 3 (Default Interface Implementation from CLAUDE.md §5.3) — the simplest default that satisfies the contract is to throw `NotImplementedException`, since there's no existing method to delegate to:
  ```csharp
  Task<Attempt<IUser?, UserOperationStatus>> UpdateCurrentUserAsync(CurrentUserUpdateModel model)
      => throw new NotImplementedException();
  // TODO (V18): Remove the default implementation.
  ```

- **`src/Umbraco.Cms.Api.Management/Factories/IUserPresentationFactory.cs:36`**: Same issue — `CreateUpdateCurrentUserModelAsync` added without a default implementation. Apply Pattern 3:
  ```csharp
  Task<CurrentUserUpdateModel> CreateUpdateCurrentUserModelAsync(Guid existingUserKey, UpdateCurrentUserRequestModel updateModel)
      => Task.FromResult(new CurrentUserUpdateModel { ExistingUserKey = existingUserKey, LanguageIsoCode = updateModel.LanguageIsoCode });
  // TODO (V18): Remove the default implementation.
  ```

- **`src/Umbraco.Core/Services/UserService.cs:833`**: `UpdateCurrentUserAsync` does not validate `LanguageIsoCode`. The sibling `UpdateAsync` calls `ValidateUserUpdateModel` which checks `_isoCodeValidator.IsValid(model.LanguageIsoCode)` and returns `UserOperationStatus.InvalidIsoCode` on failure. Without this check, an arbitrary string can be persisted as the user's language, silently breaking the UI language resolution. Add validation before calling `MapCurrentUserUpdate`:
  ```csharp
  if (_isoCodeValidator.IsValid(model.LanguageIsoCode) is false)
  {
      return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.InvalidIsoCode, null);
  }
  ```

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-modal.element.ts:27`**: `_save()` always calls `this.modalContext?.submit()` regardless of whether the profile save succeeded. If the API call fails, the modal closes silently and the user sees no feedback. The repository's `updateProfile` returns `{ error }` on failure — check the result and either show a notification or prevent submission:
  ```typescript
  private async _save() {
      const result = await this._profileSettings?.save();
      if (result?.error) return; // stay open on failure
      this.modalContext?.submit();
  }
  ```
  Also update `UmbCurrentUserWorkspaceProfileSettingsElement.save()` to return the result of `updateProfile`.

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-avatar.element.ts:68`**: Avatar upload errors are swallowed with `console.error(error)` only — no user-facing notification. The `deleteAvatar` path has the same gap (the notification is added in the repository, but upload failures from `#temporaryFileManager` and the subsequent `uploadCurrentUserAvatar` are not surfaced as a visible toast). The `changePassword` and `deleteAvatar` flows in the same repository use `this.notificationContext?.peek('danger', ...)` for failures — apply the same pattern here.

- **`src/Umbraco.Cms.Api.Management/Controllers/User/Current/UpdateCurrentUserProfileController.cs:145`**: The action method signature omits `CancellationToken`. Every other write action in this folder (`SetAvatarCurrentUserController`, `ChangePasswordCurrentUserController`) includes it as the first parameter. Add it for consistency and to allow ASP.NET Core to cancel in-flight requests:
  ```csharp
  public async Task<IActionResult> UpdateCurrentUser(CancellationToken cancellationToken, UpdateCurrentUserRequestModel model)
  ```

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-profile-settings.element.ts:50`**: `_languageIsoCode` is initialized to `''` and then set from the observed user. If the context hasn't resolved by the time `save()` is called (edge case on slow connections), an empty string is sent to the API. Consider guarding: `if (!this._languageIsoCode) return;`.

- **`src/Umbraco.Core/Services/UserService.cs:840`**: The `UpdateCurrentUserAsync` does not fire `UserSavingNotification`/`UserSavedNotification`, while `UpdateAsync` does. If any external notification handlers observe user saves (e.g., for audit logging or cache invalidation), they will be bypassed for current-user profile updates. Consider whether parity is desired here.

---

## Request Changes

Critical and important issues must be addressed first.
