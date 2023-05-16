import { isManifestJSType, isManifestLoaderType } from './type-guards';
import type { ManifestWithLoader } from './types';

export async function loadExtension<T = unknown>(manifest: ManifestWithLoader<T>): Promise<T | null> {
	try {
		if (isManifestLoaderType<T>(manifest)) {
			return manifest.loader();
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
