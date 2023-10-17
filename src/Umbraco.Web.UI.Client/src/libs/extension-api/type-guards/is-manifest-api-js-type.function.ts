import type { ManifestBase, ManifestWithLoader } from '../types.js';

export type ManifestApiJSType<T> = ManifestWithLoader<T> & { apiJs: string };
export function isManifestApiJSType<T>(manifest: ManifestBase | unknown): manifest is ManifestApiJSType<T> {
	return (manifest as ManifestApiJSType<T>).apiJs !== undefined;
}
