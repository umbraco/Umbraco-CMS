import type { UmbRepositoryResponse } from '../types.js';

export interface UmbDuplicateRepository {
	duplicate(unique: string, targetUnique: string): Promise<UmbRepositoryResponse<string>>;
}
