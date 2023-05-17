import type { DataSourceResponse } from 'src/libs/repository';

export interface UmbCopyDataSource {
	copy(unique: string, targetUnique: string | null): Promise<DataSourceResponse<string>>;
}
