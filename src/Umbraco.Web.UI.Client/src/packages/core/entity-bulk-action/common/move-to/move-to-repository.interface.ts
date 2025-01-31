import type { UmbRepositoryErrorResponse } from '../../../repository/types.js';
import type { UmbBulkMoveToRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkMoveToRepository extends UmbApi {
	requestBulkMoveTo(args: UmbBulkMoveToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
