import { UmbRepositoryErrorResponse } from './detail-repository.interface';

export interface UmbMoveRepository {
	move(unique: string, targetUnique: string): Promise<UmbRepositoryErrorResponse>;
}
