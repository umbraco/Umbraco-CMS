import UmbCollectionAliasCondition from './collection-alias.condition.js';
import { UmbCollectionHasItemsCondition } from './collection-has-items.condition.js';
import { UMB_COLLECTION_ALIAS_CONDITION, UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Collection Alias Condition',
		alias: UMB_COLLECTION_ALIAS_CONDITION,
		api: UmbCollectionAliasCondition,
	},
	{
		type: 'condition',
		name: 'Collection Has Items Condition',
		alias: UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS,
		api: UmbCollectionHasItemsCondition,
	},
];
