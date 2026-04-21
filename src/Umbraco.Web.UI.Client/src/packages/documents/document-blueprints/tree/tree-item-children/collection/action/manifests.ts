import { UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'create',
		name: 'Document Blueprint Tree Item Children Collection Create Action',
		alias: 'Umb.CollectionAction.DocumentBlueprintTreeItemChildren.Create',
		conditions: [{ alias: UMB_COLLECTION_ALIAS_CONDITION, match: UMB_DOCUMENT_BLUEPRINT_TREE_ITEM_CHILDREN_COLLECTION_ALIAS }],
	},
];
