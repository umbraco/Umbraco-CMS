import type { UmbRepositoryErrorResponse } from '../../../repository/types.js';
import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDuplicateToRepository extends UmbApi {
	requestDuplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
