import type { ManifestBase, ManifestWithLoader } from '../types.js';

export type ManifestLoaderType<T> = ManifestWithLoader<T> & {
	loader: () => Promise<T>;
};

export function isManifestLoaderType<T>(manifest: ManifestBase): manifest is ManifestLoaderType<T> {
	return typeof (manifest as ManifestLoaderType<T>).loader === 'function';
}
