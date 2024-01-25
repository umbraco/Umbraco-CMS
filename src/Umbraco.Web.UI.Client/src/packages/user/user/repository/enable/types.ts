import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbEnableUserDataSource {
	enable(userIds: string[]): Promise<UmbDataSourceErrorResponse>;
}
