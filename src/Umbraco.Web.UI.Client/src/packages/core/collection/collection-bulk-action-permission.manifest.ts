import type { UmbCollectionBulkActionPermissions } from './types.js';
import type { ManifestCondition, UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type CollectionBulkActionPermissionConditionConfig = UmbConditionConfigBase<
	typeof UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION
> & {
	match: (permissions: UmbCollectionBulkActionPermissions) => boolean;
};

export const UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION = 'Umb.Condition.CollectionBulkActionPermission';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Collection Bulk Action Permission Condition',
	alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
	api: () => import('./collection-bulk-action-permission.condition.js'),
};
