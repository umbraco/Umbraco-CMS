import type { ManifestAppEntryPoint } from '../extensions/app-entry-point.extension.js';
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
 * Extension initializer for the `appEntryPoint` extension type
 */
export class UmbAppEntryPointExtensionInitializer extends UmbExtensionInitializerBase<
	'appEntryPoint',
	ManifestAppEntryPoint
> {
	#instanceMap = new Map<string, UmbEntryPointModule>();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestAppEntryPoint>) {
		super(host, extensionRegistry, 'appEntryPoint' satisfies ManifestAppEntryPoint['type']);
	}

	async instantiateExtension(manifest: ManifestAppEntryPoint) {
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

	async unloadExtension(manifest: ManifestAppEntryPoint): Promise<void> {
		const moduleInstance = this.#instanceMap.get(manifest.alias);

		if (!moduleInstance) return;

		if (hasOnUnloadExport(moduleInstance)) {
			moduleInstance.onUnload(this.host, this.extensionRegistry);
		}

		this.#instanceMap.delete(manifest.alias);
	}
}
