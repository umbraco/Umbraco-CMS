import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDisableUserDataSource {
	disable(userIds: string[]): Promise<UmbDataSourceErrorResponse>;
}
