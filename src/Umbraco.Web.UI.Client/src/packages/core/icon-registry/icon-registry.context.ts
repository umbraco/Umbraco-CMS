import { UmbIconRegistry } from './icon.registry.js';
import type { ManifestIcons, UmbIconDefinition } from './types.js';
import { UMB_ICON_REGISTRY_CONTEXT } from './icon-registry.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { loadManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbIconRegistryContext extends UmbContextBase {
	#registry: UmbIconRegistry;
	#manifestMap = new Map();
	#icons = new UmbArrayState<UmbIconDefinition>([], (x) => x.name);
	readonly icons = this.#icons.asObservable();
	readonly approvedIcons = this.#icons.asObservablePart((icons) => icons.filter((x) => x.hidden !== true));

	constructor(host: UmbControllerHost) {
		super(host, UMB_ICON_REGISTRY_CONTEXT);
		this.#registry = new UmbIconRegistry();
		this.#registry.attach(host.getHostElement());

		this.observe(this.icons, (icons) => {
			//if (icons.length > 0) {
			this.#registry.setIcons(icons);
			//}
		});

		this.observe(umbExtensionsRegistry.byType('icons'), (manifests) => {
			manifests.forEach((manifest) => {
				if (this.#manifestMap.has(manifest.alias)) return;
				this.#manifestMap.set(manifest.alias, manifest);
				// TODO: Should we unInit a entry point if is removed?
				this.instantiateIcons(manifest);
			});
		});
	}

	async instantiateIcons(manifest: ManifestIcons) {
		if (manifest.js) {
			const js = await loadManifestPlainJs<{ default?: any }>(manifest.js);
			if (!js || !js.default || !Array.isArray(js.default)) {
				throw new Error('Icon manifest JS-file must export an array of icons as the default export.');
			}
			this.#icons.append(js.default);
		}
	}
}

export { UmbIconRegistryContext as api };
