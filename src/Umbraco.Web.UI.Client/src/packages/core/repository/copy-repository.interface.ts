import type { DataSourceResponse } from './data-source/index.js';

export interface UmbCopyRepository {
	copy(unique: string, targetUnique: string): Promise<DataSourceResponse<string>>;
}
