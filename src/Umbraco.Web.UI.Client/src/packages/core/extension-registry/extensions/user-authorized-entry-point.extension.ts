import type { ManifestPlainJs, UmbEntryPointModule } from '@umbraco-cms/backoffice/extension-api';

/**
 * Manifest for a `userAuthorizedEntryPoint`, which is loaded once the user has been authorized
 * and the current user data is available. Unlike `appEntryPoint` and `backofficeEntryPoint`,
 * extensions of this type can rely on `UMB_CURRENT_USER_CONTEXT` being populated in `onInit`.
 *
 * `onUnload` is called when the user session ends (sign-out or timeout) or the extension is
 * unregistered. If the user authorizes again, `onInit` runs again — note the current user may
 * then be a different user than in the previous session.
 *
 * This type of extension gives full control and will simply load the specified JS file.
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry.
 */
export interface ManifestUserAuthorizedEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'userAuthorizedEntryPoint';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalUserAuthorizedEntryPointExtension: ManifestUserAuthorizedEntryPoint;
	}
}
