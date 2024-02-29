import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../tree/manifests.js';
import type { ManifestMenuItemTreeKind } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItemTreeKind = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DocumentTypes',
	name: 'Document Types Menu Item',
	weight: 900,
	meta: {
		treeAlias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
		label: 'Document Types',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
