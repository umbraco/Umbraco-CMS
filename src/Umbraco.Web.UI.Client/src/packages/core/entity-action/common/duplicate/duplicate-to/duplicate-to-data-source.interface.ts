import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateToDataSource {
	duplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
