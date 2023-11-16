import { UMB_MEMBER_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.Member',
	name: 'Members Menu Item',
	weight: 400,
	meta: {
		label: 'Members',
		icon: 'icon-folder',
		treeAlias: UMB_MEMBER_TREE_ALIAS,
		menus: ['Umb.Menu.Members'],
	},
};

export const manifests = [menuItem];
