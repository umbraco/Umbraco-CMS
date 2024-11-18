import type { ManifestPlainJs, UmbEntryPointModule } from '@umbraco-cms/backoffice/extension-api';

/**
 * Manifest for an `appEntryPoint`, which is loaded up front when the app starts.
 *
 * This type of extension gives full control and will simply load the specified JS file.
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry.
 * This is useful for extensions that need to be loaded up front, like an `authProvider`.
 */
export interface ManifestAppEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'appEntryPoint';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalAppEntryPointExtension: ManifestAppEntryPoint;
	}
}
