import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.MemberTypes',
	name: 'Member Types Menu Item',
	weight: 700,
	meta: {
		label: 'Member Types',
		icon: 'umb:folder',
		treeAlias: 'Umb.Tree.MemberTypes',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
