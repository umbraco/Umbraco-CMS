import { UMB_COLLECTION_ALIAS_CONDITION } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Collection Alias Condition',
		alias: UMB_COLLECTION_ALIAS_CONDITION,
		api: () => import('./collection-alias.condition.js'),
	},
];
