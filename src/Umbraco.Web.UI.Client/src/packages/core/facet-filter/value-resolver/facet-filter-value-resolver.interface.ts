import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import type { UmbDisplayBatchResolver } from '@umbraco-cms/backoffice/display-batch-resolver';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

export interface UmbFacetFilterValueDisplayModel {
	name: string;
}

/**
 * Resolves the active values of one facet filter into what is displayed for them, for example on active filter chips.
 *
 * Results must be returned in the same positional order as the input `values` array.
 * Implement `asObservable` on the result to opt in to reactive updates.
 */
export interface UmbFacetFilterValueResolver<TValue = unknown> extends UmbDisplayBatchResolver<
	TValue,
	UmbFacetFilterValueDisplayModel
> {
	/**
	 * The manifest of the facet filter this resolver resolves values for. Set by the coordinator when the resolver is created.
	 */
	manifest?: ManifestFacetFilter;
}

type UmbFacetFilterValueResolverModule =
	| { valueResolver: ClassConstructor<UmbFacetFilterValueResolver> }
	| { default: ClassConstructor<UmbFacetFilterValueResolver> };

export type UmbFacetFilterValueResolverLoaderProperty =
	| (() => Promise<UmbFacetFilterValueResolverModule>)
	| ClassConstructor<UmbFacetFilterValueResolver>;
