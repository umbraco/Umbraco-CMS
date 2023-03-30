import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.PartialViews',
	name: 'Partial View Menu Item',
	weight: 40,
	meta: {
		label: 'Partial Views',
		icon: 'umb:folder',
		entityType: 'partial-view',
		treeAlias: 'Umb.Tree.PartialViews',
	},
	conditions: {
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
