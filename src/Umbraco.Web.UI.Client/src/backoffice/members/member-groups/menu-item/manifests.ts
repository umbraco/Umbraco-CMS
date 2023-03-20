import type { ManifestTypes } from '@umbraco-cms/models';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'Umb.Kind.Tree',
	alias: 'Umb.MenuItem.MemberGroups',
	name: 'Member Groups Menu Item',
	weight: 800,
	meta: {
		label: 'Member Groups',
		icon: 'umb:folder',
		treeAlias: 'Umb.Tree.MemberGroups',
	},
	conditions: {
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
