import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbDuplicateRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDuplicateRepository extends UmbApi {
	requestDuplicate(args: UmbDuplicateRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
