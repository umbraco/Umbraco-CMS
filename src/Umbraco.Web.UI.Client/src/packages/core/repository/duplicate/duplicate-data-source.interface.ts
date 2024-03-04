import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateDataSource {
	duplicate(unique: string, targetUnique: string | null): Promise<UmbDataSourceResponse<string>>;
}
