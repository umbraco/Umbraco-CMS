import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaFacetFilter {
	label: string;
}

export interface ManifestFacetFilter
	extends ManifestElementAndApi<any, any>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'facetFilter';
	meta: MetaFacetFilter;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFacetFilter: ManifestFacetFilter;
	}
}
