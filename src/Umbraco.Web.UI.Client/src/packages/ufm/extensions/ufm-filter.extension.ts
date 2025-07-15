import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbUfmFilterApi extends UmbApi {
	filter(...args: Array<unknown>): string | undefined | null;
}

export interface MetaUfmFilter {
	alias: string;
}

export interface ManifestUfmFilter extends ManifestApi<UmbUfmFilterApi> {
	type: 'ufmFilter';
	meta: MetaUfmFilter;
}

declare global {
	interface UmbExtensionManifestMap {
		umbUfmFilter: ManifestUfmFilter;
	}
}
