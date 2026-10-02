import type { UmbBulkMoveToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkMoveToRepository extends UmbApi {
	/**
	 * @param args The uniques to move and the destination.
	 * @param abortSignal Aborts the request for the item currently in flight and stops before starting the next one.
	 */
	requestBulkMoveTo(args: UmbBulkMoveToRequestArgs, abortSignal?: AbortSignal): Promise<UmbRepositoryErrorResponse>;
}
