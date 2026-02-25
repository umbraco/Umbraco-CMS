import type { UmbRollbackVersionDetailModel, UmbRollbackVersionItemModel } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbRollbackRepository extends UmbApi {
	requestVersionsByEntityId(
		id: string,
		culture?: string,
	): Promise<{ data?: { items: Array<UmbRollbackVersionItemModel>; total: number }; error?: unknown }>;

	requestVersionById(id: string): Promise<{ data?: UmbRollbackVersionDetailModel; error?: unknown }>;

	setPreventCleanup(versionId: string, preventCleanup: boolean): Promise<{ error?: unknown }>;

	rollback(versionId: string, culture?: string): Promise<{ error?: unknown }>;
}
