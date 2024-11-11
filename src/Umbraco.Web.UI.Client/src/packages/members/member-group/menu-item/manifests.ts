import { UMB_MEMBER_MANAGEMENT_MENU_ALIAS } from '../../section/menu/constants.js';
import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.MemberGroups',
		name: 'Member Groups Menu Item',
		weight: 100,
		meta: {
			label: '#treeHeaders_memberGroups',
			icon: 'icon-users',
			entityType: UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE,
			menus: [UMB_MEMBER_MANAGEMENT_MENU_ALIAS],
		},
	},
];
