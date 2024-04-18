import type { ManifestAppEntryPoint } from '../types/index.js';
import { hasInitExport, hasOnUnloadExport, loadManifestPlainJs } from '../functions/index.js';
import type { UmbEntryPointModule } from '../models/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbExtensionInitializerBase } from './extension-initializer-base.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

/**
 * Extension initializer for the `appEntryPoint` extension type
 */
export class UmbAppEntryPointExtensionInitializer extends UmbExtensionInitializerBase<
	'appEntryPoint',
	ManifestAppEntryPoint
> {
	#jsInstance?: UmbEntryPointModule;

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestAppEntryPoint>) {
		super(host, extensionRegistry, 'appEntryPoint');
	}

	async instantiateExtension(manifest: ManifestAppEntryPoint) {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			// If the extension has known exports, be sure to run those
			if (hasInitExport(js)) {
				js.onInit(this.host, this.extensionRegistry);
			}
		}
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	async unloadExtension(_manifest: ManifestAppEntryPoint): Promise<void> {
		if (this.#jsInstance && hasOnUnloadExport(this.#jsInstance)) {
			this.#jsInstance.onUnload(this.host, this.extensionRegistry);
		}
	}
}
