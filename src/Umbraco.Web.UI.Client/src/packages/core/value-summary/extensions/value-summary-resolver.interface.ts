import type {
	UmbDisplayBatchResolveResult,
	UmbDisplayBatchResolver,
} from '@umbraco-cms/backoffice/display-batch-resolver';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbValueSummaryResolveResult<TResolved = unknown> extends UmbDisplayBatchResolveResult<TResolved> {}

/**
 * Batch resolver for value summaries: resolves the raw values of one value type before they are rendered.
 *
 * Results must be returned in the same positional order as the input `values` array.
 * Implement `asObservable` on the result to opt in to reactive updates.
 */
export interface UmbValueSummaryResolver<TValue = unknown, TResolved = unknown> extends UmbDisplayBatchResolver<
	TValue,
	TResolved
> {
	resolveValues(values: ReadonlyArray<TValue>): Promise<UmbValueSummaryResolveResult<TResolved>>;
}
