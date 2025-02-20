import type { UmbRepositoryErrorResponse } from '../../../repository/types.js';
import type { UmbBulkTrashRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkTrashRepository extends UmbApi {
	requestBulkTrash(args: UmbBulkTrashRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
