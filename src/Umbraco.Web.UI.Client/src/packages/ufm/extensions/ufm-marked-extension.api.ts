import { ufm } from '../plugins/marked-ufm.plugin.js';
import type { UfmPlugin } from '../plugins/types.js';
import type { ManifestUfmComponent } from './ufm-component.extension.js';
import type { UmbMarkedExtensionApi } from './marked-extension.extension.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { Marked } from '@umbraco-cms/backoffice/external/marked';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

export class UmbUfmMarkedExtensionApi implements UmbMarkedExtensionApi {
	constructor(host: UmbControllerHost, marked: Marked) {
		new UmbExtensionsApiInitializer(host, umbExtensionsRegistry, 'ufmComponent', [], undefined, (controllers) => {
			marked.use(
				ufm(
					controllers
						.map((controller) => {
							const ctrl = controller as unknown as UmbExtensionApiInitializer<ManifestUfmComponent>;
							if (!ctrl.manifest || !ctrl.api) return;
							return {
								alias: ctrl.manifest.meta.alias || ctrl.manifest.alias,
								marker: ctrl.manifest.meta.marker,
								render: ctrl.api.render,
							};
						})
						.filter((x) => x) as Array<UfmPlugin>,
				),
			);
		});
	}

	destroy() {}
}

export default UmbUfmMarkedExtensionApi;
