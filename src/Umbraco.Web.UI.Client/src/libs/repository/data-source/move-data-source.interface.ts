import type { UmbDataSourceErrorResponse } from 'src/libs/repository';

export interface UmbMoveDataSource {
	move(unique: string, targetUnique: string | null): Promise<UmbDataSourceErrorResponse>;
}
