import type { UmbDuplicateRequestArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateDataSource {
	duplicate(args: UmbDuplicateRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
