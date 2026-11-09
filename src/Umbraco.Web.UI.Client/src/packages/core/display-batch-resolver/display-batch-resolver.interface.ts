import type { ClassConstructor, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbDisplayBatchResolveResult<TResolved = unknown> {
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
 * Batch resolver for transforming raw values into resolved models.
 *
 * Results must be returned in the same positional order as the input `values` array.
 * Implement `asObservable` on the result to opt in to reactive updates.
 *
 * Return models rather than display strings for anything that can vary by culture;
 * the element rendering the value picks the name.
 */
export interface UmbDisplayBatchResolver<TValue = unknown, TResolved = unknown> extends UmbApi {
	resolveValues(values: ReadonlyArray<TValue>): Promise<UmbDisplayBatchResolveResult<TResolved>>;
}

export type UmbDisplayBatchResolverModule =
	| { valueResolver: ClassConstructor<UmbDisplayBatchResolver> }
	| { default: ClassConstructor<UmbDisplayBatchResolver> };

/**
 * A batch resolver class, or a dynamic import of a module exporting it as `valueResolver` or `default`.
 */
export type UmbDisplayBatchResolverLoaderProperty =
	| (() => Promise<UmbDisplayBatchResolverModule>)
	| ClassConstructor<UmbDisplayBatchResolver>;

/**
 * Finds the resolver for a key. Returning `undefined` means values for that key are passed through unchanged.
 */
export type UmbDisplayBatchResolverLookup = (key: string) => UmbDisplayBatchResolverLoaderProperty | undefined;
