import type { ManifestFilterBase } from '../filter.extension.js';
import type { UmbDisplayBatchResolver } from '@umbraco-cms/backoffice/display-batch-resolver';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * Resolves the active values of one filter into item models, for example for active filter chips.
 * The element displaying a value uses what it needs from the item; the default element shows its `name`.
 *
 * Results must be returned in the same positional order as the input `values` array, with `undefined` for a value that
 * can't be resolved; the element then shows the raw value.
 * Implement `asObservable` on the result to opt in to reactive updates.
 */
export interface UmbFilterValueResolver<
	TValue = unknown,
	TItem extends UmbItemModel = UmbItemModel,
> extends UmbDisplayBatchResolver<TValue, TItem | undefined> {
	/**
	 * The manifest of the filter this resolver resolves values for. Set by the coordinator when the resolver is created.
	 */
	manifest?: ManifestFilterBase;
}

type UmbFilterValueResolverModule =
	| { valueResolver: ClassConstructor<UmbFilterValueResolver> }
	| { default: ClassConstructor<UmbFilterValueResolver> };

export type UmbFilterValueResolverLoaderProperty =
	| (() => Promise<UmbFilterValueResolverModule>)
	| ClassConstructor<UmbFilterValueResolver>;
