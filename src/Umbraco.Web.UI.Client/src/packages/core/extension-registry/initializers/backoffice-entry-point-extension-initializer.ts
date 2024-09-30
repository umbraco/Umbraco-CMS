import type { ManifestBackofficeEntryPoint } from '../extensions/backoffice-entry-point.extension.js';
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
 * Extension initializer for the `backofficeEntryPoint` extension type
 */
export class UmbBackofficeEntryPointExtensionInitializer extends UmbExtensionInitializerBase<
	'backofficeEntryPoint',
	ManifestBackofficeEntryPoint
> {
	#instanceMap = new Map<string, UmbEntryPointModule>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBackofficeEntryPoint>) {
		super(host, extensionRegistry, 'backofficeEntryPoint' satisfies ManifestBackofficeEntryPoint['type']);
	}

	async instantiateExtension(manifest: ManifestBackofficeEntryPoint) {
		if (manifest.js) {
			const moduleInstance = await loadManifestPlainJs(manifest.js);

			if (!moduleInstance) return;

			this.#instanceMap.set(manifest.alias, moduleInstance);

			// If the extension has known exports, be sure to run those
			if (hasInitExport(moduleInstance)) {
				await moduleInstance.onInit(this.host, this.extensionRegistry);
			}
		}
	}

	async unloadExtension(manifest: ManifestBackofficeEntryPoint): Promise<void> {
		const moduleInstance = this.#instanceMap.get(manifest.alias);

		if (!moduleInstance) return;

		if (hasOnUnloadExport(moduleInstance)) {
			moduleInstance.onUnload(this.host, this.extensionRegistry);
		}

		this.#instanceMap.delete(manifest.alias);
	}
}
