import { UmbRepositoryResponse } from './detail-repository.interface';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<UmbRepositoryResponse<string>>;
}
