import type { ManifestBase, ManifestWithLoader } from '@umbraco-cms/backoffice/extensions-registry';

export type ManifestJSType<T> = ManifestWithLoader<T> & { js: string };
export function isManifestJSType<T>(manifest: ManifestBase | unknown): manifest is ManifestJSType<T> {
	return (manifest as ManifestJSType<T>).js !== undefined;
}
