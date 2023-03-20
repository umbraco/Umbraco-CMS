import type { ManifestTypes } from '@umbraco-cms/models';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'Umb.Kind.Tree',
	alias: 'Umb.MenuItem.MemberTypes',
	name: 'Member Types Menu Item',
	weight: 30,
	meta: {
		label: 'Member Types',
		icon: 'umb:folder',
		treeAlias: 'Umb.Tree.MemberTypes',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
