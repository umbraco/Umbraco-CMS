import type { UmbCollectionBulkActionPermissions } from '../types.js';
import type { UMB_COLLECTION_ALIAS_CONDITION, UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export type CollectionBulkActionPermissionConditionConfig = UmbConditionConfigBase<
	typeof UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION
> & {
	match: (permissions: UmbCollectionBulkActionPermissions) => boolean;
};

export type CollectionAliasConditionConfig = UmbConditionConfigBase<typeof UMB_COLLECTION_ALIAS_CONDITION> & {
	/**
	 * The collection that this extension should be available in
	 * @example
	 * "Umb.Collection.User"
	 */
	match: string;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		CollectionBulkActionPermissionConditionConfig: CollectionBulkActionPermissionConditionConfig;
		CollectionAliasConditionConfig: CollectionAliasConditionConfig;
	}
}
