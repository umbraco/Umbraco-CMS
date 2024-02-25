import type { UmbDataSourceErrorResponse } from '../data-source-response.interface.js';

export interface UmbMoveRepository {
	move(unique: string, targetUnique: string): Promise<UmbDataSourceErrorResponse>;
}
