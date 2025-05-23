import type { UmbRepositoryErrorResponse } from '../../../repository/types.js';
import type { UmbBulkTrashRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
/**
 * @deprecated since 15.3.0. Will be removed in 17.0.0. Call trash method on UmbRecycleBin repositories instead.
 * @exports
 * @interface UmbBulkTrashRepository
 * @augments UmbApi
 */
export interface UmbBulkTrashRepository extends UmbApi {
	requestBulkTrash(args: UmbBulkTrashRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
