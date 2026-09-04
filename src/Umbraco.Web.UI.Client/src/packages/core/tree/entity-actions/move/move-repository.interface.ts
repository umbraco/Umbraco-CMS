import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbMoveRepository extends UmbApi {
	/**
	 * @param args The unique to move and the destination.
	 * @param abortSignal Aborts the request.
	 */
	requestMoveTo(args: UmbMoveToRequestArgs, abortSignal?: AbortSignal): Promise<UmbRepositoryErrorResponse>;
}
