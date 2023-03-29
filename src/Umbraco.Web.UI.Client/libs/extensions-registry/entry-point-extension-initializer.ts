import type { ManifestEntrypoint } from './models';
import { hasInitExport, loadExtension, UmbExtensionRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbEntryPointExtensionInitializer {
	#rootHost;
	#extensionRegistry;

	constructor(rootHost: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry) {
		this.#rootHost = rootHost;
		this.#extensionRegistry = extensionRegistry;
		// TODO: change entrypoint extension to be entryPoint:
		extensionRegistry.extensionsOfType('entrypoint').subscribe((entryPoints) => {
			entryPoints.forEach((entryPoint) => {
				this.instantiateEntryPoint(entryPoint);
			});
		});
	}

	instantiateEntryPoint(manifest: ManifestEntrypoint) {
		loadExtension(manifest).then((js) => {
			// If the extension has an onInit export, be sure to run that or else let the module handle itself
			if (hasInitExport(js)) {
				js.onInit(this.#rootHost, this.#extensionRegistry);
			}
		});
	}
}
