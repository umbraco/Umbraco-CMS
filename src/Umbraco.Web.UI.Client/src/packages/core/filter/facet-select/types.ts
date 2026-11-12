import type { ManifestFilter, MetaFilter } from '../filter.extension.js';

export interface ManifestFilterFacetSelectKind extends ManifestFilter {
	type: 'filter';
	kind: 'facetSelect';
	meta: MetaFilterFacetSelect;
}

export interface MetaFilterFacetSelect extends MetaFilter {
	/**
	 * The alias of the item repository that resolves the facet values and the selected values into items.
	 */
	itemRepositoryAlias: string;
	multiple?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFilterFacetSelectKind: ManifestFilterFacetSelectKind;
	}
}
