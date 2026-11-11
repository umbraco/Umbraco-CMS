import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * API of a facet filter active value: one per displayed value. Receives `alias`, `unique` and `value` from the element
 * rendering the active filters.
 */
export interface UmbFacetFilterActiveValueApi<TValue = unknown, TResolvedValue = unknown> extends UmbApi {
	/**
	 * The alias of the facet filter the value belongs to.
	 */
	alias?: string;
	/**
	 * The unique of the active filter entry.
	 */
	unique?: string;
	/**
	 * The raw active value.
	 */
	value?: TValue;
	/**
	 * The value resolved by the `valueResolver` of the extension, or the raw value when there is none.
	 */
	readonly resolvedValue: Observable<TResolvedValue | undefined>;
	/**
	 * The label of the facet filter the value belongs to.
	 */
	readonly filterLabel: Observable<string | undefined>;
	/**
	 * Removes the value from the active filters.
	 */
	remove(): void;
}
