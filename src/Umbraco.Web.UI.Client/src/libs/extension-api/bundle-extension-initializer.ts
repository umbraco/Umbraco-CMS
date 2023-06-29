import type { ManifestBase, ManifestBundle } from './types.js';
import { loadExtension } from './load-extension.function.js';
import { UmbExtensionRegistry } from './registry/extension.registry.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbBundleExtensionInitializer {
	#extensionRegistry;
	#bundleMap = new Map();

	constructor(host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		this.#extensionRegistry = extensionRegistry;
		extensionRegistry.extensionsOfType('bundle').subscribe((bundles) => {
			// Unregister removed bundles:
			this.#bundleMap.forEach((existingBundle) => {
				if (!bundles.find((b) => b.alias === existingBundle.alias)) {
					this.unregisterBundle(existingBundle);
					this.#bundleMap.delete(existingBundle.alias);
				}
			});

			// Register new bundles:
			bundles.forEach((bundle) => {
				if (this.#bundleMap.has(bundle.alias)) return;
				this.#bundleMap.set(bundle.alias, bundle);
				this.instantiateBundle(bundle);
			});
		});
	}

	async instantiateBundle(manifest: ManifestBundle) {
		const js = await loadExtension(manifest);

		if (js) {
			Object.keys(js).forEach((key) => {
				const value = js[key];

				if (Array.isArray(value)) {
					this.#extensionRegistry.registerMany(value);
				} else if (typeof value === 'object') {
					this.#extensionRegistry.register(value);
				}
			});
		}
	}

	async unregisterBundle(manifest: ManifestBundle) {
		const js = await loadExtension(manifest);

		if (js) {
			Object.keys(js).forEach((key) => {
				const value = js[key];

				if (Array.isArray(value)) {
					this.#extensionRegistry.unregisterMany(value.map((v) => v.alias));
				} else if (typeof value === 'object') {
					this.#extensionRegistry.unregister((value as ManifestBase).alias);
				}
			});
		}
	}
}
