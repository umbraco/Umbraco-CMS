import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * API interface for batch-resolving raw values to display values.
 *
 * Results must be returned in the same order as the input `values` array.
 * Each entry in the returned array corresponds positionally to the input.
 */
export interface UmbValueSummaryApi<TValue = unknown, TResolved = unknown> extends UmbApi {
	resolveValues(values: ReadonlyArray<TValue>): Promise<ReadonlyArray<TResolved>>;
}
