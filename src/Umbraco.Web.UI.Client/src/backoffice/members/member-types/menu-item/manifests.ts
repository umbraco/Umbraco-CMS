import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
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
