import type { ManifestEntryPoint } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { UmbEntryPointModule } from '../models/entry-point.interface.js';
import { hasBeforeInitExport, hasInitExport, loadManifestPlainJs } from '../functions/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

export class UmbEntryPointExtensionInitializer extends UmbControllerBase {
	#host;
	#extensionRegistry;
	#entryPointMap = new Map();

	constructor(
		host: UmbElement,
		extensionRegistry: UmbExtensionRegistry<ManifestEntryPoint>,
		initFn: keyof UmbEntryPointModule,
	) {
		super(host);
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		this.observe(extensionRegistry.byType('entryPoint'), (entryPoints) => {
			entryPoints.forEach((entryPoint) => {
				if (this.#entryPointMap.has(entryPoint.alias)) return;
				this.#entryPointMap.set(entryPoint.alias, entryPoint);
				// TODO: Should we unInit a entry point if is removed?
				this.instantiateEntryPoint(entryPoint, initFn);
			});
		});
	}

	async instantiateEntryPoint(manifest: ManifestEntryPoint, initFn: keyof UmbEntryPointModule = 'onInit') {
		if (manifest.js) {
			const js = await loadManifestPlainJs(manifest.js);

			// If the extension has known exports, be sure to run those
			if (initFn === 'beforeInit' && hasBeforeInitExport(js)) {
				js.beforeInit(this.#host, this.#extensionRegistry);
			} else if (initFn === 'onInit' && hasInitExport(js)) {
				js.onInit(this.#host, this.#extensionRegistry);
			}
		}
	}
}
