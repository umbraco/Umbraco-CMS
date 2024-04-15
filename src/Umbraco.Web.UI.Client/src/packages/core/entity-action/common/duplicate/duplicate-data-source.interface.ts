import type { UmbDuplicateToRequestArgs } from './duplicate-to/types.js';
import type { UmbDuplicateRequestArgs } from './duplicate/types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateDataSource {
	duplicate(args: UmbDuplicateRequestArgs): Promise<UmbDataSourceErrorResponse>;
	duplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
