import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbMoveDataSource {
	move(unique: string, targetUnique: string): Promise<UmbDataSourceErrorResponse>;
}
