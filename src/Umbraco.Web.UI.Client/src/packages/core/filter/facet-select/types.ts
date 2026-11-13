import type { MetaFilter } from '../filter.extension.js';

export interface MetaFilterFacetSelect extends MetaFilter {
	/**
	 * The alias of the item repository that resolves the facet values and the selected values into items.
	 */
	itemRepositoryAlias: string;
	multiple?: boolean;
}
