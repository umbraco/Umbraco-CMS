import type { UmbFacetFilterValueResolverLoaderProperty } from '../value-resolver/facet-filter-value-resolver.interface.js';
import type { UmbFacetFilterActiveValueApi } from './facet-filter-active-value-api.interface.js';
import type { UmbFacetFilterActiveValueElement } from './facet-filter-active-value-element.interface.js';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Displays the active values of one or more facet filters, for example as removable chips above a collection.
 * Without a matching registration, an active value is displayed by the `default` kind.
 */
export interface ManifestFacetFilterActiveValue extends ManifestElementAndApi<
	UmbFacetFilterActiveValueElement,
	UmbFacetFilterActiveValueApi
> {
	type: 'facetFilterActiveValue';
	/**
	 * The aliases of the facet filters whose active values this extension displays.
	 */
	forFacetFilters: Array<string>;
	/**
	 * Optional resolver for the active values before they are displayed, for example from IDs to items.
	 * Created once per facet filter by the facet filter value coordinator, so values are batched across all displayed
	 * values of the filter. Without a resolver, the element receives the raw value.
	 */
	valueResolver?: UmbFacetFilterValueResolverLoaderProperty;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFacetFilterActiveValue: ManifestFacetFilterActiveValue;
	}
}
