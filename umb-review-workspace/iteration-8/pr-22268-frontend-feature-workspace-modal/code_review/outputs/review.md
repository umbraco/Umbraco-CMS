### Code review

Found 6 issues:

1. **Breaking change: `IUserService.UpdateCurrentUserAsync` added without a default implementation** (CLAUDE.md §5.3)
   `src/Umbraco.Core/Services/IUserService.cs` (~line 95 in diff)

2. **Wrong response/error types in `putUserCurrentProfile` SDK method** (incorrect copy-paste from `PutUserById`)
   `src/Umbraco.Web.UI.Client/src/packages/core/backend-api/sdk.gen.ts` (~line 388 in diff)

3. **Wrong response/error types in `deleteUserCurrentAvatar` SDK method** (incorrect copy-paste from `DeleteUserAvatarById`)
   `src/Umbraco.Web.UI.Client/src/packages/core/backend-api/sdk.gen.ts` (~line 367 in diff)

4. **Modal `_save()` calls `submit()` unconditionally even when save fails**
   `src/Umbraco.Web.UI.Client/src/packages/user/current-user/modals/current-user-workspace/current-user-workspace-modal.element.ts` (~lines 805-808 in diff)

5. **Hardcoded English notification strings instead of localization keys**
   `src/Umbraco.Web.UI.Client/src/packages/user/current-user/repository/current-user.repository.ts` (~lines 1115, 1133 in diff)

6. **Trailing whitespace introduced in `UserService.cs`**
   `src/Umbraco.Core/Services/UserService.cs` (~line 341 in diff)
