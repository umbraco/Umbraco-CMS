import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbValueMinimalDisplayApi<TValue = unknown, TResolved = unknown> extends UmbApi {
	resolveValues(values: ReadonlyArray<TValue>): Promise<Map<string, TResolved>>;
}
