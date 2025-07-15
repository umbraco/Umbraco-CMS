import type { ManifestBase, ManifestBundle } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { loadManifestPlainJs } from '../functions/load-manifest-plain-js.function.js';
import { UmbExtensionInitializerBase } from './extension-initializer-base.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

/**
 * Extension initializer for the `bundle` extension type
 */
export class UmbBundleExtensionInitializer extends UmbExtensionInitializerBase<'bundle', ManifestBundle> {
	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBundle>) {
		super(host, extensionRegistry, 'bundle');
	}

	async instantiateExtension(manifest: ManifestBundle): Promise<void> {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			if (js) {
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
}
