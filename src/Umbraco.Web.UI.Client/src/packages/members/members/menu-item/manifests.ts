import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Members',
	name: 'Members Menu Item',
	weight: 400,
	meta: {
		label: 'Members',
		icon: 'umb:folder',
		entityType: 'member',
		treeAlias: 'Umb.Tree.Members',
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
