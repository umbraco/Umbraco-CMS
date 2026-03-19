import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaCollectionFacetFilter {
	label: string;
}

export interface ManifestCollectionFacetFilter
	extends ManifestElementAndApi<any, any>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'collectionFacetFilter';
	meta: MetaCollectionFacetFilter;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionFacetFilter: ManifestCollectionFacetFilter;
	}
}
