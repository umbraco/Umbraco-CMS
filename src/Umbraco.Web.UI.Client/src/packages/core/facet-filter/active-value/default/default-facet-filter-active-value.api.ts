import { UmbFacetFilterActiveValueApiBase } from '../base/facet-filter-active-value-api.base.js';

/**
 * Default facet filter active value API. Provided by the `default` kind for `facetFilterActiveValue` extensions.
 */
export class UmbDefaultFacetFilterActiveValueApi<
	TValue = unknown,
	TResolvedValue = unknown,
> extends UmbFacetFilterActiveValueApiBase<TValue, TResolvedValue> {}
