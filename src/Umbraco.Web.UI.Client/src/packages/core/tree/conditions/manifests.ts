import { UmbTreeAliasCondition } from './tree-alias.condition.js';
import { UMB_TREE_ALIAS_CONDITION } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Tree Alias Condition',
		alias: UMB_TREE_ALIAS_CONDITION,
		api: UmbTreeAliasCondition,
	},
];
