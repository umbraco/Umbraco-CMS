import type { UmbSortChildrenOfArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbSortChildrenOfDataSource {
	sortChildrenOf(args: UmbSortChildrenOfArgs): Promise<UmbDataSourceErrorResponse>;
}
