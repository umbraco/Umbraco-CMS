import type { ManifestFacetFilter, MetaFacetFilter } from '../facet-filter.extension.js';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestFacetFilterSelectKind extends ManifestFacetFilter {
	type: 'facetFilter';
	kind: 'select';
	meta: MetaFacetFilterSelect;
}

export interface MetaFacetFilterSelect extends MetaFacetFilter {
	datalistDataSource?: new (host: UmbControllerHost) => UmbDatalistDataSource;
	multiple?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFacetFilterSelectKind: ManifestFacetFilterSelectKind;
	}
}
