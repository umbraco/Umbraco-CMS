import type { UmbFacetFilterValueResolverLoaderProperty } from './value-resolver/facet-filter-value-resolver.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface MetaFacetFilter {
	label: string;
}

export interface ManifestFacetFilter
	extends ManifestElementAndApi<any, any>, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'facetFilter';
	meta: MetaFacetFilter;
	/**
	 * Optional resolver for what is displayed for the filter's active values, for example on active filter chips.
	 * Created once per filter, separately from the filter's element and API. Kinds can provide a default.
	 * Without a resolver, the raw value is displayed.
	 */
	valueResolver?: UmbFacetFilterValueResolverLoaderProperty;
}

declare global {
	interface UmbExtensionManifestMap {
		umbFacetFilter: ManifestFacetFilter;
	}
}
