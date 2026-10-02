import type { UmbBulkDuplicateToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkDuplicateToRepository extends UmbApi {
	/**
	 * @param args The uniques to duplicate and the destination.
	 * @param abortSignal Aborts the request for the item currently in flight and stops before starting the next one.
	 */
	requestBulkDuplicateTo(
		args: UmbBulkDuplicateToRequestArgs,
		abortSignal?: AbortSignal,
	): Promise<UmbRepositoryErrorResponse>;
}
