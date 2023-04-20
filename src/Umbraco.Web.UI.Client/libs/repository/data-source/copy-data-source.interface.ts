import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbCopyDataSource {
	copy(unique: string, targetUnique: string): Promise<DataSourceResponse<string>>;
}
