# Code Review: PR #22268 - Current User Workspace Modal

**Branch**: `origin/pr/22268`
**Base**: `origin/main`
**Files changed**: 29 files, 960 insertions / 8 deletions
**Type**: Medium frontend feature with backend endpoints

---

## Summary

This PR adds a "Current User Workspace" sidebar modal that allows users without access to the Users section to edit their own profile (language preference) and avatar. It introduces two new backend API endpoints (`DELETE /user/current/avatar` and `PUT /user/current/profile`), a new section condition (`SectionUserNoPermission`), and the complete frontend implementation as a Lit web component modal.

The overall direction is sound and the feature is a genuine UX improvement. However, several issues need to be addressed before merging.

---

## Breaking Changes

### CRITICAL: `IUserService.UpdateCurrentUserAsync` added without default implementation

**File**: `src/Umbraco.Core/Services/IUserService.cs`

A new method `Task<Attempt<IUser?, UserOperationStatus>> UpdateCurrentUserAsync(CurrentUserUpdateModel model)` is added to the public `IUserService` interface **without a default implementation**. This is a binary breaking change for any external consumers who have implemented `IUserService` themselves (e.g., for mocking in tests, or alternative implementations).

Per the project's breaking change policy (CLAUDE.md §5.3), any new method added to a public interface must have a default implementation:

```csharp
// Required default implementation to avoid breaking external implementors
Task<Attempt<IUser?, UserOperationStatus>> UpdateCurrentUserAsync(CurrentUserUpdateModel model)
    => throw new NotImplementedException();
    // or delegate to UpdateAsync with appropriate defaults
```

The `CurrentUserUpdateModel` is also a new public class in `Umbraco.Core`, which is fine on its own, but the missing interface default is the blocker.

---

## Bugs

### HIGH: Incorrect response/error types in generated SDK methods

**File**: `src/Umbraco.Web.UI.Client/src/packages/core/backend-api/sdk.gen.ts`

The two new SDK methods use response/error types from the wrong (admin) endpoints:

**`deleteUserCurrentAvatar`** uses:
```typescript
client.delete<DeleteUserAvatarByIdResponses, DeleteUserAvatarByIdErrors, ThrowOnError>
```
...instead of the correct `DeleteUserCurrentAvatarResponses` / `DeleteUserCurrentAvatarErrors` types that were defined for it in `types.gen.ts`.

**`putUserCurrentProfile`** uses:
```typescript
client.put<PutUserByIdResponses, PutUserByIdErrors, ThrowOnError>
```
...instead of the correct `PutUserCurrentProfileResponses` / `PutUserCurrentProfileErrors` types. (`PutUserByIdErrors` and `PutUserByIdResponses` are the types for `PUT /user/{id}`, the admin endpoint.)

The correct `DeleteUserCurrentAvatarResponses` / `DeleteUserCurrentAvatarErrors` types exist in `types.gen.ts` but are not referenced. This looks like a copy-paste error during manual SDK editing. This means TypeScript's type checking will not catch mismatches between what the new endpoints actually return and what the SDK signals to callers.

### HIGH: Language ISO code not validated in `UpdateCurrentUserAsync`

**File**: `src/Umbraco.Core/Services/UserService.cs`

The existing `UpdateAsync` method validates the language ISO code via `_isoCodeValidator.IsValid(model.LanguageIsoCode)` and returns `UserOperationStatus.InvalidIsoCode` if invalid. The new `UpdateCurrentUserAsync` method skips this validation entirely:

```csharp
// Missing: if (_isoCodeValidator.IsValid(model.LanguageIsoCode) is false)
//     return Attempt.FailWithStatus<IUser?, UserOperationStatus>(UserOperationStatus.InvalidIsoCode, null);

IUser updated = MapCurrentUserUpdate(model, existingUser);
UserOperationStatus saveStatus = await userStore.SaveAsync(updated);
```

An empty string (the default for `UpdateCurrentUserRequestModel.LanguageIsoCode`) or any arbitrary string can be saved. The `UpdateCurrentUserRequestModel` has no validation attributes (`[Required]`, `[StringLength]`) either, so an empty or invalid language code can reach the database.

### MEDIUM: `URL.createObjectURL` memory leak — no `URL.revokeObjectURL` call

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts`

After a successful avatar upload, a blob URL is created and stored in the user store:
```typescript
const localUrl = URL.createObjectURL(file);
this.#currentUserStore?.update({ avatarUrls: [localUrl, localUrl, localUrl, localUrl, localUrl] });
```

`URL.createObjectURL` allocates memory that is never freed because `URL.revokeObjectURL(localUrl)` is never called. Since the blob URL is duplicated 5 times in the store and the repository is long-lived, this is a real memory leak. The blob URL should be revoked once the real server URLs are loaded (i.e., after `requestCurrentUser()` is called on next page load or after re-fetching user data), or at minimum when the avatar URL is replaced.

### MEDIUM: No error handling when `_save` fails in the modal

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-modal.element.ts`

The `_save` method calls `_profileSettings?.save()` and then unconditionally calls `this.modalContext?.submit()` regardless of whether the save succeeded:

```typescript
private async _save() {
    await this._profileSettings?.save();
    this.modalContext?.submit();  // Always called, even on failure
}
```

If the `updateProfile` API call fails, the modal closes as if the save succeeded. The user receives no feedback that their changes were not saved. The `save()` method in `UmbCurrentUserWorkspaceProfileSettingsElement` also silently discards errors.

Compare with the existing `changePassword` flow which shows a danger notification on failure. The avatar upload has a `try/catch` block in `#uploadAvatar` but only logs to console and does not show any user-facing error notification.

### LOW: `#abortController` created but never used for actual cancellation

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts`

A class-level `#abortController = new AbortController()` is created but `abort()` is never called (e.g., in a `destroy()` lifecycle hook). If the user navigates away during an upload, the upload will continue in the background with no way to cancel it. Either use the controller properly (abort on destroy) or remove it.

### LOW: Trailing blank line + whitespace in `UserService.cs`

**File**: `src/Umbraco.Core/Services/UserService.cs`

The diff shows two blank lines and a trailing whitespace were added at the end of the file before the `#endregion` marker. This is a minor formatting issue that should be cleaned up before merge (dotnet format will catch this).

---

## Architectural Issues

### MEDIUM: Condition logic inverted — `SectionUserNoPermission` is unnecessary

**File**: `src/Umbraco.Web.UI.Client/src/packages/core/section/conditions/section-user-no-permission/`

A new negating condition `UmbSectionUserNoPermissionCondition` is introduced specifically to show the "Edit" button only to users who do NOT have access to the Users section. This is the inverse of the existing `UmbSectionUserPermissionCondition`.

Adding a generic negation condition to the core package just to serve this one use case is an architectural smell. Instead:

1. The existing `UmbSectionUserPermissionCondition` could gain a `negate: boolean` config property (common pattern in extensible condition systems).
2. Or the condition could be scoped to the `user/current-user` package rather than `core/section/conditions`.

The new condition adds 4 files to `core/section/conditions` and exports to the `@umbraco-cms/backoffice/section` barrel export, which increases surface area for an inverted single-use condition.

### MEDIUM: `UpdateCurrentUserProfileController` missing `CancellationToken` parameter

**File**: `src/Umbraco.Cms.Api.Management/Controllers/User/Current/UpdateCurrentUserProfileController.cs`

`ClearAvatarCurrentUserController.ClearAvatar` accepts a `CancellationToken` parameter (consistent with other controllers in the codebase), but `UpdateCurrentUserProfileController.UpdateCurrentUser` does not. This is an inconsistency. The `CancellationToken` is unused in `ClearAvatar`, which is also odd — if it's there for consistency it should also be in `UpdateCurrentUser`.

### LOW: `CreateUpdateCurrentUserModelAsync` is needlessly async

**File**: `src/Umbraco.Cms.Api.Management/Factories/UserPresentationFactory.cs`

The new factory method returns `Task.FromResult(model)` wrapping a synchronous operation. The factory interface method signature `Task<CurrentUserUpdateModel> CreateUpdateCurrentUserModelAsync(...)` forces async on a method that doesn't need it. This should be either:
- Synchronous (returns `CurrentUserUpdateModel` directly), or
- Named consistently with the sync/async designation actually needed

The existing `CreateItemResponseModel` is synchronous. The async suffix here is misleading.

### LOW: Hardcoded comment about server returning 5 avatar sizes

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts`

```typescript
// The server returns 5 different sizes of the avatar, so we mimic that here
this.#currentUserStore?.update({ avatarUrls: [localUrl, localUrl, localUrl, localUrl, localUrl] });
```

Using the same blob URL 5 times (hardcoded) as a local optimistic update is fragile. If the server changes the number of avatar sizes, this will silently provide the wrong count. A more robust approach: call `requestCurrentUser()` immediately after upload to fetch the actual URLs from the server, and show a loading state in the interim.

---

## Code Quality

### Label attribute binding inconsistency in `current-user-workspace-avatar.element.ts`

The "change photo" button uses string interpolation (`label="${...}"`), while the "remove photo" button uses property binding (`label=${...}`). In Lit, `.label=${...}` (property binding) is preferred for non-string values; for string values, either works but the team should be consistent. The modal element correctly uses `.label=${...}` for both its buttons.

```html
<!-- Inconsistent: string interpolation -->
<uui-button label="${this.localize.term('user_changePhoto')}" ...>

<!-- Consistent with rest of codebase: property binding -->
<uui-button .label=${this.localize.term('user_removePhoto')} ...>
```

### Hardcoded notification messages not localized

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts`

The success notification messages `"Avatar uploaded"` and `"Avatar deleted"` are hardcoded English strings, not localization keys. Existing notifications in the same file (e.g., for password change) use `error.message` for failure messages but those too are not localized. At minimum, these should follow the pattern established elsewhere in the codebase.

### `AvatarUploadForm` form element is unused

**File**: `src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-avatar.element.ts`

The `<form id="AvatarUploadForm" novalidate>` wrapper element does not participate in form submission — uploads are triggered imperatively via the `#uploadAvatar` handler. The form wrapper and its `novalidate` attribute add no value and could be replaced with a `<div>`.

### `_currentUser` is observed in both modal element and avatar element independently

**Files**: `current-user-workspace-modal.element.ts`, `current-user-workspace-avatar.element.ts`, `current-user-workspace-profile-settings.element.ts`

All three elements separately consume `UMB_CURRENT_USER_CONTEXT` and observe `currentUser`. The modal element fetches `_currentUser` only to display the headline name, while the child elements also independently fetch it. This creates three separate observers and three copies of the same data in memory. The modal could pass the user name as a property to child elements, or a single observation at the top level could be sufficient.

### Test: First test uses variable `modal` instead of `model`

**File**: `tests/Umbraco.Tests.Integration/Umbraco.Core/Services/UserServiceCrudTests.Update.cs`

```csharp
var modal = new CurrentUserUpdateModel  // "modal" is a typo for "model"
```

The second test correctly uses `model`. This is a minor naming inconsistency.

---

## Summary Table

| Severity | Issue | File |
|----------|-------|------|
| CRITICAL | `IUserService.UpdateCurrentUserAsync` lacks default implementation (breaking change) | `IUserService.cs` |
| HIGH | Wrong response/error types in `deleteUserCurrentAvatar` and `putUserCurrentProfile` SDK methods | `sdk.gen.ts` |
| HIGH | No ISO code validation in `UpdateCurrentUserAsync` | `UserService.cs` |
| MEDIUM | `URL.createObjectURL` memory leak — no `URL.revokeObjectURL` | `current-user.repository.ts` |
| MEDIUM | `_save()` closes modal on error without user feedback | `current-user-workspace-modal.element.ts` |
| MEDIUM | Inverted condition added to core section package for single use case | `section-user-no-permission/` |
| MEDIUM | `UpdateCurrentUser` missing `CancellationToken` parameter (inconsistency) | `UpdateCurrentUserProfileController.cs` |
| LOW | `#abortController` created but never cancelled on destroy | `current-user.repository.ts` |
| LOW | `CreateUpdateCurrentUserModelAsync` is needlessly async | `UserPresentationFactory.cs` |
| LOW | Hardcoded 5 avatar sizes in optimistic update | `current-user.repository.ts` |
| LOW | Trailing blank lines / whitespace added to `UserService.cs` | `UserService.cs` |
| LOW | `label="..."` vs `.label=...` inconsistency in avatar element | `current-user-workspace-avatar.element.ts` |
| LOW | Hardcoded English notification messages | `current-user.repository.ts` |
| LOW | Unused `<form>` wrapper in avatar element | `current-user-workspace-avatar.element.ts` |
| LOW | Test variable named `modal` instead of `model` | `UserServiceCrudTests.Update.cs` |
