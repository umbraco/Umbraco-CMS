import type { UmbRepositoryErrorResponse } from '../types.js';

export interface UmbMoveRepository {
	move(unique: string, targetUnique: string): Promise<UmbRepositoryErrorResponse>;
}
