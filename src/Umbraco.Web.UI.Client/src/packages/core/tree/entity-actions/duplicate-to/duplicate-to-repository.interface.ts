import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDuplicateToRepository extends UmbApi {
	requestDuplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
