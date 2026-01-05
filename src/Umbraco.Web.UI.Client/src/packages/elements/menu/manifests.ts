import { UMB_ELEMENT_TREE_ALIAS } from '../tree/constants.js';
import { UMB_ELEMENT_MENU_ITEM_ALIAS } from './constants.js';
import { UMB_LIBRARY_MENU_ALIAS } from '@umbraco-cms/backoffice/library';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'tree',
		alias: UMB_ELEMENT_MENU_ITEM_ALIAS,
		name: 'Element Menu Item',
		weight: 100,
		meta: {
			treeAlias: UMB_ELEMENT_TREE_ALIAS,
			label: '#general_elements',
			menus: [UMB_LIBRARY_MENU_ALIAS],
		},
	},
];
