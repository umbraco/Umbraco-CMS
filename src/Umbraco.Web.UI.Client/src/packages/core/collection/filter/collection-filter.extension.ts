import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaCollectionFilter {
	label: string;
	filterKey: string;
}
export interface ManifestCollectionFilter
	extends ManifestElementAndApi<any, any>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionFilter';
	meta: MetaCollectionFilter;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionFilter: ManifestCollectionFilter;
	}
}
