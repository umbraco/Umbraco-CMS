import type { ManifestPlainJs, UmbEntryPointModule } from '@umbraco-cms/backoffice/extension-api';

/**
 * Manifest for an `backofficeEntryPoint`, which is loaded after the Backoffice has been loaded and authentication has been done.
 *
 * This type of extension gives full control and will simply load the specified JS file.
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry.
 */
export interface ManifestBackofficeEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'backofficeEntryPoint';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbGlobalBackofficeEntryPointExtension: ManifestBackofficeEntryPoint;
	}
}
