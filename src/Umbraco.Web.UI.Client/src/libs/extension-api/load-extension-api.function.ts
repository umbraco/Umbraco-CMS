import { isManifestApiJSType, isManifestJSType, isManifestLoaderType } from './type-guards/index.js';
import type { ManifestWithLoader } from './types.js';

export async function loadExtensionApi<T = unknown>(manifest: ManifestWithLoader<T>): Promise<T | null> {
	try {
		if (isManifestLoaderType<T>(manifest)) {
			return manifest.loader();
		}

		if (isManifestApiJSType<T>(manifest) && manifest.apiJs) {
			return await import(/* @vite-ignore */ manifest.apiJs);
		}

		if (isManifestJSType<T>(manifest) && manifest.js) {
			return await import(/* @vite-ignore */ manifest.js);
		}
	} catch (err: any) {
		console.warn('-- Extension failed to load script', manifest, err);
		return Promise.resolve(null);
	}

	return Promise.resolve(null);
}
