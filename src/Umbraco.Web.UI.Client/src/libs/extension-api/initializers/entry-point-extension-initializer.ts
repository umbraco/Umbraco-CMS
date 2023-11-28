import type { ManifestEntryPoint } from '../types/index.js';
import { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { hasInitExport, loadManifestPlainJs } from '../functions/index.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbElement } from '@umbraco-cms/backoffice/element-api';

export class UmbEntryPointExtensionInitializer extends UmbBaseController {
	#host;
	#extensionRegistry;
	#entryPointMap = new Map();

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>) {
		super(host);
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		this.observe(extensionRegistry.extensionsOfType('entryPoint'), (entryPoints) => {
			entryPoints.forEach((entryPoint) => {
				if (this.#entryPointMap.has(entryPoint.alias)) return;
				this.#entryPointMap.set(entryPoint.alias, entryPoint);
				// TODO: Should we unInit a entry point if is removed?
				this.instantiateEntryPoint(entryPoint);
			});
		});
	}

	async instantiateEntryPoint(manifest: ManifestEntryPoint) {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);
			// If the extension has an onInit export, be sure to run that or else let the module handle itself
			if (hasInitExport(js)) {
				js.onInit(this.#host, this.#extensionRegistry);
			}
		}
	}
}
