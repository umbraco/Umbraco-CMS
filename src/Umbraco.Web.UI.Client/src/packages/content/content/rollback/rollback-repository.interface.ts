import type { UmbContentRollbackVersionDetailModel, UmbContentRollbackVersionItemModel } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbContentRollbackRepository extends UmbApi {
	requestVersionsByEntityId(
		id: string,
		culture?: string,
	): Promise<UmbRepositoryResponse<{ items: Array<UmbContentRollbackVersionItemModel>; total: number }>>;

	requestVersionById(id: string): Promise<UmbRepositoryResponse<UmbContentRollbackVersionDetailModel>>;

	setPreventCleanup(versionId: string, preventCleanup: boolean): Promise<UmbRepositoryResponse<never>>;

	rollback(versionId: string, culture?: string): Promise<UmbRepositoryResponse<never>>;
}
