import { EXAMPLE_TREE_ALIAS } from '../tree/constants.js';
import { UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: 'Example.MenuItem.Tree',
		name: 'Example Tree Menu Item',
		weight: 1000,
		meta: {
			label: 'Example Tree',
			menus: [UMB_CONTENT_MENU_ALIAS],
			treeAlias: EXAMPLE_TREE_ALIAS,
		},
	},
];
