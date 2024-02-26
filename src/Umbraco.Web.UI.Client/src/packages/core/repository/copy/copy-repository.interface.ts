import type { DataSourceResponse } from '../data-source-response.interface.js';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<DataSourceResponse<string>>;
}
