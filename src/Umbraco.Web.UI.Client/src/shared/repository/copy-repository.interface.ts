import { UmbRepositoryResponse } from './detail-repository.interface.js';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<UmbRepositoryResponse<string>>;
}
