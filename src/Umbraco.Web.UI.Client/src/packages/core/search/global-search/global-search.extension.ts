import type { UmbGlobalSearchApi } from './types.js';
import type { ManifestWithDynamicConditions, ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestGlobalSearch
	extends ManifestApi<UmbGlobalSearchApi>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'globalSearch';
	meta: MetaGlobalSearch;
}

export interface MetaGlobalSearch {
	label: string;
	searchProviderAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbGlobalSearch: ManifestGlobalSearch;
	}
}
