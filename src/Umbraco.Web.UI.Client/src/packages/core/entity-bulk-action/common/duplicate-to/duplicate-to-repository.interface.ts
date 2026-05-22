import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbBulkDuplicateToRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBulkDuplicateToRepository extends UmbApi {
	requestBulkDuplicateTo(args: UmbBulkDuplicateToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
