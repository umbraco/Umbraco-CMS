import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Documents',
	name: 'Documents Menu Item',
	weight: 200,
	meta: {
		label: 'Documents',
		icon: 'icon-folder',
		menus: ['Umb.Menu.Content'],
		treeAlias: 'Umb.Tree.Document',
		hideTreeRoot: true,
	},
};

export const manifests = [menuItem];
