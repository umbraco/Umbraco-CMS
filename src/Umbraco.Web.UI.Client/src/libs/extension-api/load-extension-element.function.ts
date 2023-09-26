import { isManifestElementJSType, isManifestJSType, isManifestLoaderType } from './type-guards/index.js';
import type { ManifestWithLoader } from './types.js';

export async function loadExtensionElement<T = unknown>(manifest: ManifestWithLoader<T>): Promise<T | null> {
	try {
		if (isManifestLoaderType<T>(manifest)) {
			return manifest.loader();
		}

		if (isManifestElementJSType<T>(manifest) && manifest.elementJs) {
			return await import(/* @vite-ignore */ manifest.elementJs);
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
