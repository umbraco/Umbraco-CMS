import { UmbApi } from '../models/api.interface.js';
import { isManifestApiJSType, isManifestJSType, isManifestLoaderType } from '../type-guards/index.js';
import type { ManifestApi } from '../types/index.js';

export async function loadExtensionApi<T extends UmbApi = UmbApi>(manifest: ManifestApi<T>): Promise<T | null> {
	try {

		// TODO: Get rid of this, instead make apiJs support loader.
		if (isManifestLoaderType<T>(manifest)) {
			return manifest.loader();
		}

		if (isManifestApiJSType<T>(manifest)) {
			return await import(/* @vite-ignore */ manifest.apiJs);
		}

		if (isManifestJSType<T>(manifest)) {
			return await import(/* @vite-ignore */ manifest.js);
		}
	} catch (err: any) {
		console.warn('-- Extension failed to load script', manifest, err);
		return Promise.resolve(null);
	}

	return Promise.resolve(null);
}
