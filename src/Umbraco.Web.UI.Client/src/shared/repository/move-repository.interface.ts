import { UmbRepositoryErrorResponse } from './detail-repository.interface.js';

export interface UmbMoveRepository {
	move(unique: string, targetUnique: string): Promise<UmbRepositoryErrorResponse>;
}
