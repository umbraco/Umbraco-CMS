import { UMB_CONTENT_MENU_ALIAS } from '../../menu.manifests.js';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DocumentRecycleBin',
	name: 'Document Recycle Bin Menu Item',
	weight: 100,
	meta: {
		treeAlias: 'Umb.Tree.DocumentRecycleBin',
		label: 'Recycle Bin',
		icon: 'icon-trash',
		menus: [UMB_CONTENT_MENU_ALIAS],
	},
};

export const manifests = [menuItem];
