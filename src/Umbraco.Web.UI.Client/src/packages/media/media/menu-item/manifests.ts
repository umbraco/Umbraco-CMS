import { UMB_MEDIA_MENU_ALIAS } from '../menu.manifests.js';
import { UMB_MEDIA_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Media',
	name: 'Media Menu Item',
	weight: 100,
	meta: {
		label: 'Media',
		menus: [UMB_MEDIA_MENU_ALIAS],
		treeAlias: UMB_MEDIA_TREE_ALIAS,
		hideTreeRoot: true,
	},
};

export const manifests = [menuItem];
