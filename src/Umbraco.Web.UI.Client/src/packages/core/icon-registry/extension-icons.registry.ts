import { UmbIconRegistry } from './icon.registry.js';
import type { UmbIconDefinition } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { loadManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { type ManifestIcons, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbExtensionIconsRegistry extends UmbControllerBase {
	//#host: UmbControllerHost;
	#registry: UmbIconRegistry;
	#manifestMap = new Map();
	#icons = new UmbArrayState<UmbIconDefinition>([], (x) => x.name);
	readonly icons = this.#icons.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		//this.#host = host;
		this.#registry = new UmbIconRegistry();
		this.#registry.attach(host.getHostElement());

		this.observe(this.icons, (icons) => {
			this.#registry.setIcons(icons);
		});

		this.observe(umbExtensionsRegistry.byType('icons'), (manifests) => {
			manifests.forEach((manifest) => {
				if (this.#manifestMap.has(manifest.alias)) return;
				this.#manifestMap.set(manifest.alias, manifest);
				// TODO: Should we unInit a entry point if is removed?
				this.instantiateEntryPoint(manifest);
			});
		});
	}

	async instantiateEntryPoint(manifest: ManifestIcons) {
		if (manifest.js) {
			const js = await loadManifestPlainJs<{ default?: any }>(manifest.js);
			if (!js || !js.default || !Array.isArray(js.default)) {
				throw new Error('Icon manifest JS-file must export an array of icons as the default export.');
			}
			this.#icons.append(js.default);
		}
	}
}
