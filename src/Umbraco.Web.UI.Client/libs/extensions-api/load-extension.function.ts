import type { ManifestElement } from '../models';
import { isManifestJSType } from './is-manifest-js-type.function';
import { isManifestLoaderType } from './is-manifest-loader-type.function';

export type ManifestLoaderType = ManifestElement & { loader: () => Promise<object | HTMLElement> };
export type ManifestJSType = ManifestElement & { js: string };

export async function loadExtension(manifest: ManifestElement): Promise<object | HTMLElement | null> {
	try {
		if (isManifestLoaderType(manifest)) {
			return manifest.loader();
		}

		if (isManifestJSType(manifest) && manifest.js) {
			return await import(/* @vite-ignore */ manifest.js);
		}
	} catch {
		console.warn('-- Extension failed to load script', manifest);
		return Promise.resolve(null);
	}

	return Promise.resolve(null);
}