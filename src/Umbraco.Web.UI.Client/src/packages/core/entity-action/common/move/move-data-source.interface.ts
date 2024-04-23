import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbMoveDataSource {
	moveTo(args: UmbMoveToRequestArgs): Promise<UmbDataSourceErrorResponse>;
}
