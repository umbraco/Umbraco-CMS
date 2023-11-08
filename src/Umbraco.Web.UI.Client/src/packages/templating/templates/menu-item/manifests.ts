import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Templates',
	name: 'Templates Menu Item',
	weight: 40,
	meta: {
		label: 'Templates',
		icon: 'icon-folder',
		entityType: 'template',
		treeAlias: 'Umb.Tree.Templates',
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
