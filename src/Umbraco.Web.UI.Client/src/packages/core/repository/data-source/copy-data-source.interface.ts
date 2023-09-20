import type { DataSourceResponse } from 'src/packages/core/repository';

export interface UmbCopyDataSource {
	copy(unique: string, targetUnique: string | null): Promise<DataSourceResponse<string>>;
}
