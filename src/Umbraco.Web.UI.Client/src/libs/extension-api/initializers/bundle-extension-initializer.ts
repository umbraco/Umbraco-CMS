import type { ManifestBase, ManifestBundle } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { loadManifestPlainJs } from '../functions/load-manifest-plain-js.function.js';
import { registerExtensionModule, unregisterExtensionModule } from '../decorators/index.js';
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
	// Caches the loaded module per manifest alias so unloadExtension doesn't need to re-import.
	#loadedModules = new Map<string, Record<string, unknown>>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		super(host, extensionRegistry, 'bundle');
	}

	async instantiateExtension(manifest: ManifestBundle): Promise<void> {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			if (js) {
				this.#loadedModules.set(manifest.alias, js);

				if (registerExtensionModule(js, this.extensionRegistry)) {
					return;
				}

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
		const js = this.#loadedModules.get(manifest.alias);
		this.#loadedModules.delete(manifest.alias);

		if (js) {
			if (unregisterExtensionModule(js, this.extensionRegistry)) {
				return;
			}

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
