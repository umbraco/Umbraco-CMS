import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbMoveDataSource {
	move(unique: string, targetUnique: string | null): Promise<UmbDataSourceErrorResponse>;
}
