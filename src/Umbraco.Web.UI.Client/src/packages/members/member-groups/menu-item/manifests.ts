import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
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
