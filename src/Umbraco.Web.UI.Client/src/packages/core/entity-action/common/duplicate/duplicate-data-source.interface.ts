import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateDataSource {
	duplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
