import type { ManifestBundle } from './types.js';
import { loadExtension } from './load-extension.function.js';
import { UmbExtensionRegistry } from './registry/extension.registry.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbBundleExtensionInitializer {
	#host;
	#extensionRegistry;
	#bundleMap = new Map();

	constructor(host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		extensionRegistry.extensionsOfType('bundle').subscribe((bundles) => {
			bundles.forEach((bundle) => {
				if (this.#bundleMap.has(bundle.alias)) return;
				this.#bundleMap.set(bundle.alias, bundle);
				// TODO: Should we unInit a entry point if is removed?
				this.instantiateBundle(bundle);
			});
		});
	}

	async instantiateBundle(manifest: ManifestBundle) {
		const js = await loadExtension(manifest);

		if (js) {
			Object.keys(js).forEach((key) => {
				this.#extensionRegistry.registerMany(js[key]);
			});
		}
	}
}
