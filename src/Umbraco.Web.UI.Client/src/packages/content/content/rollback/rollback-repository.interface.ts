import type { UmbContentRollbackVersionDetailModel, UmbContentRollbackVersionItemModel } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbContentRollbackRepository extends UmbApi {
	requestVersionsByEntityId(
		id: string,
		culture?: string,
	): Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }>;

	requestVersionById(id: string): Promise<{ data?: UmbContentRollbackVersionDetailModel; error?: unknown }>;

	setPreventCleanup(versionId: string, preventCleanup: boolean): Promise<{ error?: unknown }>;

	rollback(versionId: string, culture?: string): Promise<{ error?: unknown }>;
}
