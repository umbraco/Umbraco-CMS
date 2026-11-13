import type { ManifestFilterBase, MetaFilterFacetSelect, MetaFilterSelect } from '@umbraco-cms/backoffice/filter';

/**
 * A filter of collections. It shows in the collections listed in `forCollections`.
 */
export interface ManifestCollectionFilter extends ManifestFilterBase {
	type: 'collectionFilter';
	/**
	 * The aliases of the collections this filter applies to.
	 */
	forCollections: Array<string>;
}

export interface ManifestCollectionFilterSelectKind extends ManifestCollectionFilter {
	kind: 'select';
	meta: MetaFilterSelect;
}

export interface ManifestCollectionFilterFacetSelectKind extends ManifestCollectionFilter {
	kind: 'facetSelect';
	meta: MetaFilterFacetSelect;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCollectionFilter: ManifestCollectionFilter;
		umbCollectionFilterSelectKind: ManifestCollectionFilterSelectKind;
		umbCollectionFilterFacetSelectKind: ManifestCollectionFilterFacetSelectKind;
	}
}
