import { UMB_MEMBER_MENU_ALIAS } from '../../menu.manifests.js';
import { UMB_MEMBER_GROUP_TREE_ALIAS } from '../tree/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	kind: 'tree',
	alias: 'Umb.MenuItem.MemberGroup',
	name: 'Member Group Menu Item',
	weight: 400,
	meta: {
		label: 'Member Groups',
		treeAlias: UMB_MEMBER_GROUP_TREE_ALIAS,
		menus: [UMB_MEMBER_MENU_ALIAS],
	},
};

export const manifests = [menuItem];
