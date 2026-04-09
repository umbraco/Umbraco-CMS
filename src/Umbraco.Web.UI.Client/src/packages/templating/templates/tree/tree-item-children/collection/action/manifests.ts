import { UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'create',
		name: 'Template Tree Item Children Collection Create Action',
		alias: 'Umb.CollectionAction.TemplateTreeItemChildren.Create',
		conditions: [{ alias: UMB_COLLECTION_ALIAS_CONDITION, match: UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS }],
	},
];
