import { UmbDataSourceErrorResponse } from './data-source/index.js';

export interface UmbMoveRepository {
	move(unique: string, targetUnique: string): Promise<UmbDataSourceErrorResponse>;
}
