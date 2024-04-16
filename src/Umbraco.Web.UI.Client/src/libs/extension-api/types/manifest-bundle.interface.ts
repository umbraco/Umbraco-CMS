import type { ManifestPlainJs } from './base.types.js';
import type { ManifestBase } from './manifest-base.interface.js';

/**
 * This type of extension takes a JS module and registers all exported manifests from the pointed JS file.
 */
export interface ManifestBundle<UmbManifestTypes extends ManifestBase = ManifestBase>
	extends ManifestPlainJs<{ [key: string]: Array<UmbManifestTypes> }> {
	type: 'bundle';

	/**
	 * The scope of the bundle. If global, the bundle will be loaded on the root host and never be destroyed.
	 * If local, the bundle will be loaded on the BackofficeElement and will be destroyed when the host is removed for example when signing out.
	 * @default 'local'
	 * @enum ['global', 'local']
	 */
	scope?: 'global' | 'local';
}
