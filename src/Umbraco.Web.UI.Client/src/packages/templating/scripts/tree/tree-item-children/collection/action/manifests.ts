import { UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'create',
		name: 'Script Tree Item Children Collection Create Action',
		alias: 'Umb.CollectionAction.ScriptTreeItemChildren.Create',
		conditions: [{ alias: UMB_COLLECTION_CONDITION_ALIAS, match: UMB_SCRIPT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS }],
	},
];
