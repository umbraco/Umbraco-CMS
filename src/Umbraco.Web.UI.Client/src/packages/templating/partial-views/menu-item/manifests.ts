import { PARTIAL_VIEW_TREE_ALIAS } from '../config.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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
		treeAlias: PARTIAL_VIEW_TREE_ALIAS,
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
