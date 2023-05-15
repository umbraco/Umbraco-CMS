import type { ManifestEntrypoint } from '../extensions-registry/models';
import { hasInitExport, loadExtension, UmbExtensionRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbEntryPointExtensionInitializer {
	#host;
	#extensionRegistry;
	#entryPointMap = new Map();

	constructor(host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry) {
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		extensionRegistry.extensionsOfType('entryPoint').subscribe((entryPoints) => {
			entryPoints.forEach((entryPoint) => {
				if (this.#entryPointMap.has(entryPoint.alias)) return;
				this.#entryPointMap.set(entryPoint.alias, entryPoint);
				this.instantiateEntryPoint(entryPoint);
			});
		});
	}

	async instantiateEntryPoint(manifest: ManifestEntrypoint) {
		const js = await loadExtension(manifest);
		// If the extension has an onInit export, be sure to run that or else let the module handle itself
		if (hasInitExport(js)) {
			js.onInit(this.#host, this.#extensionRegistry);
		}
	}
}
