import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface IUmbUserDetailRepository {
	uploadAvatar(id: string, file: File): Promise<UmbDataSourceErrorResponse>;
	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse>;
}
