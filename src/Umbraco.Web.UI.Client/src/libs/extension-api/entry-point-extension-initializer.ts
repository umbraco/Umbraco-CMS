import { UmbBaseController } from '../controller-api/controller.class.js';
import type { ManifestEntryPoint } from './types.js';
import { hasInitExport } from './has-init-export.function.js';
import { loadExtension } from './load-extension.function.js';
import { UmbExtensionRegistry } from './registry/extension.registry.js';
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
		const js = await loadExtension(manifest);
		// If the extension has an onInit export, be sure to run that or else let the module handle itself
		if (hasInitExport(js)) {
			js.onInit(this.#host, this.#extensionRegistry);
		}
	}
}
