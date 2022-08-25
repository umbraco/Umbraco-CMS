import { ManifestTypes } from '../models';

export type ManifestLoaderType = ManifestTypes & { loader: () => Promise<object | HTMLElement> };
export type ManifestJSType = ManifestTypes & { js: string };

export async function loadExtension(manifest: ManifestTypes): Promise<object | HTMLElement | null> {
	if (isManifestLoaderType(manifest)) {
		return manifest.loader();
	}

	if (isManifestJSType(manifest) && manifest.js) {
		try {
			return await import(/* @vite-ignore */ manifest.js);
		} catch {
			console.warn('-- Extension failed to load script', manifest.js);
			return Promise.resolve(null);
		}
	}

	return Promise.resolve(null);
}

export function isManifestLoaderType(manifest: ManifestTypes): manifest is ManifestLoaderType {
	return typeof (manifest as ManifestLoaderType).loader === 'function';
}

export function isManifestJSType(manifest: ManifestTypes): manifest is ManifestJSType {
	return (manifest as ManifestJSType).js !== undefined;
}
