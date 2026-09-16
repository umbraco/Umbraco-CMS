import type { ManifestPlainJs, UmbEntryPointModule } from '@umbraco-cms/backoffice/extension-api';

/**
 * Manifest for a `userEntryPoint`, which is loaded once the user has been authorized
 * and the current user data is available. Unlike `appEntryPoint` and `backofficeEntryPoint`,
 * extensions of this type can rely on `UMB_CURRENT_USER_CONTEXT` being populated in `onInit`.
 * For changes to the current user within the session (permissions, languages, etc.), observe UMB_CURRENT_USER_CONTEXT instead — this extension type only marks the session boundaries.
 *
 * `onUnload` is called when the user session ends (sign-out or timeout) or the extension is
 * unregistered. If the user authorizes again, `onInit` runs again — note the current user may
 * then be a different user than in the previous session.
 *
 * This type of extension gives full control and will simply load the specified JS file.
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry.
 * @example
 * ```ts
 * export const onInit: UmbEntryPointOnInit = async (host) => {
 * 	const currentUserContext = await host.getContext(UMB_CURRENT_USER_CONTEXT);
 * 	// Guaranteed to be populated — never undefined:
 * 	console.log(currentUserContext?.getAllowedSection());
 * 	// React to changes during the session:
 * 	host.observe(currentUserContext?.allowedSections, (allowedSections) => {});
 * };
 * ```
 */
export interface ManifestUserEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'userEntryPoint';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalUserEntryPointExtension: ManifestUserEntryPoint;
	}
}
