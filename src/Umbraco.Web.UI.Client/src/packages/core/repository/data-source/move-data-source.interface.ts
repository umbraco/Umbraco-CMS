import type { UmbDataSourceErrorResponse } from 'src/packages/core/repository';

export interface UmbMoveDataSource {
	move(unique: string, targetUnique: string | null): Promise<UmbDataSourceErrorResponse>;
}
