import type { ManifestEntryPoint } from '../types/index.js';
import type { UmbEntryPointModule } from '../models/index.js';
import { hasInitExport, hasOnUnloadExport, loadManifestPlainJs } from '../functions/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbExtensionInitializerBase } from './extension-initializer-base.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

/**
 * Extension initializer for the `entryPoint` extension type
 */
export class UmbEntryPointExtensionInitializer extends UmbExtensionInitializerBase<'entryPoint', ManifestEntryPoint> {
	#instanceMap = new Map<string, UmbEntryPointModule>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>) {
		super(host, extensionRegistry, 'entryPoint');
	}

	async instantiateExtension(manifest: ManifestEntryPoint) {
		if (manifest.js) {
			const moduleInstance = await loadManifestPlainJs(manifest.js);

			if (!moduleInstance) return;

			this.#instanceMap.set(manifest.alias, moduleInstance);

			// If the extension has known exports, be sure to run those
			if (hasInitExport(moduleInstance)) {
				moduleInstance.onInit(this.host, this.extensionRegistry);
			}
		}
	}

	async unloadExtension(manifest: ManifestEntryPoint): Promise<void> {
		const moduleInstance = this.#instanceMap.get(manifest.alias);

		if (!moduleInstance) return;

		if (hasOnUnloadExport(moduleInstance)) {
			moduleInstance.onUnload(this.host, this.extensionRegistry);
		}

		this.#instanceMap.delete(manifest.alias);
	}
}
