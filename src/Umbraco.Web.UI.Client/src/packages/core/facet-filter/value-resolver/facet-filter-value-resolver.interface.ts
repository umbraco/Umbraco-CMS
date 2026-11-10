import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import type { UmbDisplayBatchResolver } from '@umbraco-cms/backoffice/display-batch-resolver';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * Resolves the active values of one facet filter into item models, for example for active filter chips.
 * The element displaying a value uses what it needs from the item; the default element shows its `name`.
 *
 * Results must be returned in the same positional order as the input `values` array.
 * Implement `asObservable` on the result to opt in to reactive updates.
 */
export interface UmbFacetFilterValueResolver<
	TValue = unknown,
	TItem extends UmbItemModel = UmbItemModel,
> extends UmbDisplayBatchResolver<TValue, TItem> {
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
