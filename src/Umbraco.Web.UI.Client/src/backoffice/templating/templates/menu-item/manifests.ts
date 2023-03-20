import type { ManifestTypes } from '@umbraco-cms/models';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'Umb.Kind.Tree',
	alias: 'Umb.MenuItem.Templates',
	name: 'Templates Menu Item',
	weight: 40,
	meta: {
		label: 'Templates',
		icon: 'umb:folder',
		entityType: 'template',
		treeAlias: 'Umb.Tree.Templates',
	},
	conditions: {
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
