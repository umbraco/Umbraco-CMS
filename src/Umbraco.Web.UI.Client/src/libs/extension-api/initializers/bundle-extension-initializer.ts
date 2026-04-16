import type { ManifestBase, ManifestBundle } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { loadManifestPlainJs } from '../functions/load-manifest-plain-js.function.js';
import { getExtensionManifest, registerExtensionModule } from '../decorators/index.js';
import { UmbExtensionInitializerBase } from './extension-initializer-base.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

/**
 * Extension initializer for the `bundle` extension type.
 *
 * Handles two module formats:
 * 1. **Classic bundles:** Exports manifest arrays/objects directly.
 * 2. **Decorator bundles:** Exports classes decorated with `@umbExtension`.
 *    These are detected automatically and registered via {@link registerExtensionModule}.
 */
export class UmbBundleExtensionInitializer extends UmbExtensionInitializerBase<'bundle', ManifestBundle> {
	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		super(host, extensionRegistry, 'bundle');
	}

	async instantiateExtension(manifest: ManifestBundle): Promise<void> {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			if (js) {
				// Check if any export carries @umbExtension metadata
				if (this.#hasDecoratedExports(js)) {
					registerExtensionModule(js, this.extensionRegistry);
					return;
				}

				// Classic bundle: exports are manifest arrays/objects
				Object.keys(js).forEach((key) => {
					const value = js[key];

					if (Array.isArray(value)) {
						this.extensionRegistry.registerMany(value);
					} else if (typeof value === 'object') {
						this.extensionRegistry.register(value);
					}
				});
			}
		}
	}

	async unloadExtension(manifest: ManifestBundle): Promise<void> {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			if (js) {
				// For decorator bundles, read the alias from the metadata
				if (this.#hasDecoratedExports(js)) {
					for (const value of Object.values(js)) {
						const meta = getExtensionManifest(value);
						if (meta) {
							this.extensionRegistry.unregister(meta.alias);
						}
					}
					return;
				}

				// Classic bundle
				Object.keys(js).forEach((key) => {
					const value = js[key];

					if (Array.isArray(value)) {
						this.extensionRegistry.unregisterMany(value.map((v) => v.alias));
					} else if (typeof value === 'object') {
						this.extensionRegistry.unregister((value as ManifestBase).alias);
					}
				});
			}
		}
	}

	#hasDecoratedExports(moduleExports: Record<string, unknown>): boolean {
		return Object.values(moduleExports).some((value) => getExtensionManifest(value) !== undefined);
	}
}
