import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/** @internal */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMarkedExtensionApi extends UmbApi {}

/** @internal */
export interface MetaMarkedExtension {
	alias: string;
}

/**
 * @internal
 * @description The `markedExtension` extension-type is currently for internal use and should be considered experimental.
 */
export interface ManifestMarkedExtension extends ManifestApi<UmbMarkedExtensionApi> {
	type: 'markedExtension';
	meta: MetaMarkedExtension;
}

declare global {
	interface UmbExtensionManifestMap {
		umbMarkedExtension: ManifestMarkedExtension;
	}
}
