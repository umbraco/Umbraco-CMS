import { UMB_MEMBER_TYPE_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.MemberTypes',
	name: 'Member Type Menu Item',
	weight: 700,
	meta: {
		label: 'Member Types',
		icon: 'icon-folder',
		treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
