import type { ManifestBase, ManifestBundle } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { loadManifestPlainJs } from '../functions/load-manifest-plain-js.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBundleExtensionInitializer extends UmbControllerBase {
	#extensionRegistry;
	#bundleMap = new Map();

	constructor(host: UmbControllerHost, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		super(host);
		this.#extensionRegistry = extensionRegistry;
		this.observe(extensionRegistry.byType('bundle'), (bundles) => {
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
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

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
	}

	async unregisterBundle(manifest: ManifestBundle) {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

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
}
