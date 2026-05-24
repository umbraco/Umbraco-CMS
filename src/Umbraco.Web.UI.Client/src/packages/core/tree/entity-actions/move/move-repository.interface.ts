import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbMoveRepository extends UmbApi {
	requestMoveTo(args: UmbMoveToRequestArgs): Promise<UmbRepositoryErrorResponse>;
}
