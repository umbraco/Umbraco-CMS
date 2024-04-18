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
	#jsInstance?: UmbEntryPointModule;

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>) {
		super(host, extensionRegistry, 'entryPoint');
	}

	async instantiateExtension(manifest: ManifestEntryPoint) {
		if (manifest.js) {
			this.#jsInstance = await loadManifestPlainJs(manifest.js);

			// If the extension has known exports, be sure to run those
			if (hasInitExport(this.#jsInstance)) {
				this.#jsInstance.onInit(this.host, this.extensionRegistry);
			}
		}
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	async unloadExtension(_manifest: ManifestEntryPoint): Promise<void> {
		if (this.#jsInstance && hasOnUnloadExport(this.#jsInstance)) {
			this.#jsInstance.onUnload(this.host, this.extensionRegistry);
		}
	}
}
