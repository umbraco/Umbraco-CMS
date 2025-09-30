import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbUnlockUserDataSource {
	unlock(userIds: string[]): Promise<UmbDataSourceErrorResponse>;
}
