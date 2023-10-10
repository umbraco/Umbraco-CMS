import type { ManifestBase, ManifestWithLoader } from '../types.js';

export type ManifestElementJSType<T> = ManifestWithLoader<T> & { elementJs: string };
export function isManifestElementJSType<T>(manifest: ManifestBase | unknown): manifest is ManifestElementJSType<T> {
	return (manifest as ManifestElementJSType<T>).elementJs !== undefined;
}
