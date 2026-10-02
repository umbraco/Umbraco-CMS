import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDuplicateToRepository extends UmbApi {
	/**
	 * @param args The unique to duplicate and the destination.
	 * @param abortSignal Aborts the request.
	 */
	requestDuplicateTo(args: UmbDuplicateToRequestArgs, abortSignal?: AbortSignal): Promise<UmbRepositoryErrorResponse>;
}
