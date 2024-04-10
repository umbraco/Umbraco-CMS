import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbMoveToDataSource {
	move(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
