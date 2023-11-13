import { UmbApi } from '../models/api.interface.js';
import type { ManifestApi } from '../types/index.js';
import { loadManifestApiProperty } from './manifest-api-loader.function.js';

//TODO: Write tests for this method:
export async function createExtensionApi<ApiType extends UmbApi = UmbApi>(
	manifest: ManifestApi<ApiType>,
	constructorArguments: unknown[] = []
): Promise<ApiType | undefined> {

	if(manifest.api) {
		const api = await loadManifestApiProperty(manifest.api, constructorArguments);
		if(api) {
			return api;
		}
	} else if (manifest.js) {
		const js = await loadManifestApiProperty(manifest.js, constructorArguments);
		if(js) {
			return js;
		}
	}

	console.error(
		`-- Extension of alias "${manifest.alias}" did not succeed creating an api class instance, missing a JavaScript file via the 'apiJs' or 'js' property or a ClassConstructor in 'api' in the manifest.`,
		manifest
	);
	return undefined;
}
