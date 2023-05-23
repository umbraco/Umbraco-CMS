import type { ManifestEntryPoint } from './types';
import { hasInitExport } from './has-init-export.function';
import { loadExtension } from './load-extension.function';
import { UmbExtensionRegistry } from './registry/extension.registry';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbEntryPointExtensionInitializer {
	#host;
	#extensionRegistry;
	#entryPointMap = new Map();

	constructor(host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>) {
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		extensionRegistry.extensionsOfType('entryPoint').subscribe((entryPoints) => {
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
