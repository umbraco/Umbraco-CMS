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
	// Stores the import promise so unloadExtension can await it even if instantiate is still in flight.
	#loadingModules = new Map<string, Promise<Record<string, unknown> | undefined>>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		super(host, extensionRegistry, 'bundle');
	}

	async instantiateExtension(manifest: ManifestBundle): Promise<void> {
		if (manifest.js) {
			const jsPromise = loadManifestPlainJs(manifest.js);
			this.#loadingModules.set(manifest.alias, jsPromise);

			const js = await jsPromise;

			if (js) {
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
		const jsPromise = this.#loadingModules.get(manifest.alias);
		this.#loadingModules.delete(manifest.alias);

		const js = await jsPromise;

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
