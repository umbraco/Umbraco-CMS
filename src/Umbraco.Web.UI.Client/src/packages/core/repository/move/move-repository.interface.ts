import type { UmbRepositoryErrorResponse } from '../types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbMoveRepository extends UmbApi {
	move(unique: string, targetUnique: string): Promise<UmbRepositoryErrorResponse>;
}
