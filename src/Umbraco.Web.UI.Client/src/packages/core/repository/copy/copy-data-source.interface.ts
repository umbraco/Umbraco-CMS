import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbCopyDataSource {
	copy(unique: string, targetUnique: string | null): Promise<UmbDataSourceResponse<string>>;
}
