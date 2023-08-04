import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.RelationTypes',
	name: 'Relation Types Menu Item',
	weight: 500,
	meta: {
		treeAlias: 'Umb.Tree.RelationTypes',
		label: 'Relation Types',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
