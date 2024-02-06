import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.PartialView',
	name: 'Partial View Menu Item',
	weight: 40,
	meta: {
		label: 'Partial Views',
		treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS,
		menus: ['Umb.Menu.Templating'],
	},
};

export const manifests = [menuItem];
