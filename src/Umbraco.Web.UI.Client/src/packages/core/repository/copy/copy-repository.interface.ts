import type { UmbDataSourceResponse } from '../data-source-response.interface.js';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<UmbDataSourceResponse<string>>;
}
