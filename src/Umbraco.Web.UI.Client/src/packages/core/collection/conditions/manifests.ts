import { UMB_COLLECTION_ALIAS_CONDITION, UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Collection Alias Condition',
		alias: UMB_COLLECTION_ALIAS_CONDITION,
		api: () => import('./collection-alias.condition.js'),
	},
	/** @deprecated No longer used internally. This class will be removed in Umbraco 17. [LK] */
	{
		type: 'condition',
		name: 'Collection Bulk Action Permission Condition',
		alias: UMB_COLLECTION_BULK_ACTION_PERMISSION_CONDITION,
		api: () => import('./collection-bulk-action-permission.condition.js'),
	},
];
