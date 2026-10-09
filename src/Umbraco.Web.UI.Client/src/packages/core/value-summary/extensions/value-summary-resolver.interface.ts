import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbValueSummaryResolveResult<TResolved = unknown> {
	/**
	 * The initially resolved values, in the same order as the input.
	 */
	data: ReadonlyArray<TResolved>;
	/**
	 * Optional reactive stream that re-emits when the underlying data changes.
	 * When provided, the coordinator subscribes and updates resolved values on each emission.
	 */
	asObservable?: () => Observable<ReadonlyArray<TResolved>>;
}

/**
 * Batch resolver for transforming raw values to display values.
 *
 * Results must be returned in the same positional order as the input `values` array.
 * Implement `asObservable` on the result to opt in to reactive updates.
 */
export interface UmbValueSummaryResolver<TValue = unknown, TResolved = unknown> extends UmbApi {
	resolveValues(values: ReadonlyArray<TValue>): Promise<UmbValueSummaryResolveResult<TResolved>>;
}
