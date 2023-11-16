import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Media',
	name: 'Media Menu Item',
	weight: 100,
	meta: {
		label: 'Media',
		icon: 'icon-folder',
		menus: ['Umb.Menu.Media'],
		treeAlias: 'Umb.Tree.Media',
		hideTreeRoot: true,
	},
};

export const manifests = [menuItem];
