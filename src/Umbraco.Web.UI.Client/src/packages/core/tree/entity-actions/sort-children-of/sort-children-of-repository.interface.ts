import type { UmbSortChildrenOfArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbSortChildrenOfRepository extends UmbApi {
	sortChildrenOf(args: UmbSortChildrenOfArgs): Promise<UmbRepositoryErrorResponse>;
}
