import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaCollectionFacetFilter {
	label: string;
	filterKey: string;
	datalistDataSource: new (host: UmbControllerHost) => UmbDatalistDataSource;
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
