import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.DocumentRecycleBin',
	name: 'Document Recycle Bin Menu Item',
	weight: 100,
	meta: {
		treeAlias: 'Umb.Tree.DocumentRecycleBin',
		label: 'Recycle Bin',
		icon: 'umb:trash',
		menus: ['Umb.Menu.Content'],
	},
};

export const manifests = [menuItem];
