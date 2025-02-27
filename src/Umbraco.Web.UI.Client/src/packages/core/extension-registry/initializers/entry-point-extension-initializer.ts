import type { ManifestEntryPoint } from '../extensions/entry-point.extension.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import {
	type UmbEntryPointModule,
	UmbExtensionInitializerBase,
	type UmbExtensionRegistry,
	loadManifestPlainJs,
	hasInitExport,
	hasOnUnloadExport,
} from '@umbraco-cms/backoffice/extension-api';

/**
 * Extension initializer for the `entryPoint` extension type
 */
export class UmbEntryPointExtensionInitializer extends UmbExtensionInitializerBase<'entryPoint', ManifestEntryPoint> {
	#instanceMap = new Map<string, UmbEntryPointModule>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>) {
		super(host, extensionRegistry, 'entryPoint' satisfies ManifestEntryPoint['type']);
	}

	async instantiateExtension(manifest: ManifestEntryPoint) {
		console.error('The `entryPoint` extension-type is deprecated, please use the `backofficeEntryPoint`.');

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
