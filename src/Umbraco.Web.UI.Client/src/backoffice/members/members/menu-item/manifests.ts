import type { ManifestTypes } from '@umbraco-cms/models';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'Umb.Kind.Tree',
	alias: 'Umb.MenuItem.Members',
	name: 'Members Menu Item',
	weight: 400,
	meta: {
		label: 'Members',
		icon: 'umb:folder',
		entityType: 'member',
		treeAlias: 'Umb.Tree.Members',
	},
	conditions: {
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
