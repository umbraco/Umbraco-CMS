import type { UmbEntryPointModule } from '../models/index.js';
import type { ManifestPlainJs } from './base.types.js';

/**
 * This type of extension gives full control and will simply load the specified JS file
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry
 */
export interface ManifestEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'entryPoint';

	/**
	 * The scope of the entry point. If global, the entry point will be loaded on the root host and never be destroyed.
	 * If local, the entry point will be loaded on the BackofficeElement and will be destroyed when the host is removed for example when signing out.
	 * @default 'local'
	 * @enum ['global', 'local']
	 */
	scope?: 'global' | 'local';
}
