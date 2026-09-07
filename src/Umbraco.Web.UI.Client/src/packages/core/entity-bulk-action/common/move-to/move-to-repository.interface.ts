import type { UmbBulkMoveToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkMoveToRepository extends UmbApi {
	requestBulkMoveTo(args: UmbBulkMoveToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
