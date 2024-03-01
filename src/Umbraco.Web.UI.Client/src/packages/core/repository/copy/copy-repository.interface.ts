import type { UmbRepositoryResponse } from '../types.js';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<UmbRepositoryResponse<string>>;
}
