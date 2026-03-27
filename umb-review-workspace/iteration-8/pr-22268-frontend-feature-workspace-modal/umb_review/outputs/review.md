## PR Review

**Target:** `origin/main` · **Based on commit:** `b14f852637cd24c401a66f0361bcc5fbc6f4f731`
· **Skipped:** 2 noise files (`sdk.gen.ts`, `types.gen.ts`) out of 29 total

Adds a "Current User Workspace" modal (sidebar) for users who lack access to the Users section, letting them update their UI language and avatar. Introduces two new Management API endpoints (`PUT /user/current/profile`, `DELETE /user/current/avatar`) backed by new `IUserService.UpdateCurrentUserAsync` and a new `UmbSectionUserNoPermissionCondition` frontend extension condition.

- **Modified public API:** `IUserService` (new method `UpdateCurrentUserAsync`), `IUserPresentationFactory` (new method `CreateUpdateCurrentUserModelAsync`)
- **Affected implementations (outside this PR):** Any third-party implementation of `IUserService` or `IUserPresentationFactory` — both interfaces are public and shipped in the NuGet packages
- **Breaking changes:** Both new interface methods lack default implementations; external implementors get a compile error
- **Other changes:** New modal alias `Umb.Modal.CurrentUser.Workspace` registered in the extension registry; new condition alias `Umb.Condition.SectionUserNoPermission` exported via the public `@umbraco-cms/backoffice/section` package

---

### Critical

- **`src/Umbraco.Core/Services/IUserService.cs:100`**: `UpdateCurrentUserAsync` added to a public interface without a default implementation. Any third-party code that implements `IUserService` (e.g., custom user stores, mock services in tests) will fail to compile. Apply Pattern 3: add a default implementation — the simplest correct default is to `throw new NotImplementedException()` with a `// TODO (V19): Remove...` comment, or better, forward to a stub that returns `UserNotFound`:
  ```csharp
  Task<Attempt<IUser?, UserOperationStatus>> UpdateCurrentUserAsync(CurrentUserUpdateModel model)
      => Task.FromResult(Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.UserNotFound, default));
  // TODO (V19): Remove the default implementation when external implementors have migrated.
  ```

- **`src/Umbraco.Cms.Api.Management/Factories/IUserPresentationFactory.cs:37`**: `CreateUpdateCurrentUserModelAsync` added to a public interface without a default implementation. Same breaking change as above. A safe default:
  ```csharp
  Task<CurrentUserUpdateModel> CreateUpdateCurrentUserModelAsync(Guid existingUserKey, UpdateCurrentUserRequestModel updateModel)
      => Task.FromResult(new CurrentUserUpdateModel { ExistingUserKey = existingUserKey, LanguageIsoCode = updateModel.LanguageIsoCode });
  // TODO (V19): Remove the default implementation.
  ```

### Important

- **`src/Umbraco.Core/Services/UserService.cs` (UpdateCurrentUserAsync)**:  `LanguageIsoCode` is not validated against `_isoCodeValidator` before saving, while `UpdateAsync` does validate it and returns `UserOperationStatus.InvalidIsoCode` on failure. Any string (empty, malformed) can be persisted via `PUT /user/current/profile`. Add the same validation guard used in `UpdateAsync`:
  ```csharp
  if (_isoCodeValidator.IsValid(model.LanguageIsoCode) is false)
  {
      return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.InvalidIsoCode, existingUser);
  }
  ```
  The integration test `Can_update_current_user` uses `"da"` (a valid short-form code that passes), so the missing validation is not caught by existing tests. A test case with an invalid code should be added to `UserServiceCrudTests.Update.cs`.

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-modal.element.ts`**: The modal calls `this.modalContext?.submit()` after saving the profile (line ~27 in `_save()`), regardless of whether `save()` returned an error. If `updateProfile()` fails (network error, invalid iso code), the modal silently dismisses. The `save()` method in `current-user-workspace-profile-settings.element.ts` does not propagate errors back to the modal. Either propagate the error return value and conditionally call `submit()`, or handle the error in the profile settings element and notify the user without closing the modal.

- **Missing tests for new API endpoints**: `ClearAvatarCurrentUserController` and `UpdateCurrentUserProfileController` are new public API endpoints. No integration tests for authorization, success, or error paths are included. Existing similar controllers (e.g. `ClearAvatarUserControllerTests`) have authorization tests — these new current-user variants should follow the same pattern.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts` (uploadAvatar)**: The `#abortController` is an instance-level field initialized once in the constructor. If two concurrent uploads are initiated, the first call's controller is shared. Since the `uploadAvatar` call is user-triggered (file picker), this is unlikely in practice, but creating the `AbortController` per call inside `uploadAvatar` would be safer and consistent with how other parts of the codebase handle it.

- **`src/Umbraco.Core/Services/UserService.cs` (ClearAvatarAsync)**: The method does not use a `ICoreScope` (it uses only `IServiceScope`), which means the avatar file deletion and the user save are not wrapped in a database transaction. If `DeleteFile` succeeds but a subsequent operation rolls back (or vice versa), the state diverges. Compare with `SetAvatarAsync` which uses `ICoreScope`. Consider wrapping in `ScopeProvider.CreateCoreScope()` and completing it only after both operations succeed — or at minimum add a comment explaining why the asymmetry is intentional.

- **`src/Umbraco.Cms.Api.Management/Controllers/User/Current/UpdateCurrentUserProfileController.cs:145`**: The action method signature is `UpdateCurrentUser(UpdateCurrentUserRequestModel model)` but the controller class name is `UpdateCurrentUserProfileController`. Minor naming inconsistency — consider `UpdateProfile` for the method name to match the route (`/profile`) and the controller name.

---

## Request Changes

Critical and important issues must be addressed first.
