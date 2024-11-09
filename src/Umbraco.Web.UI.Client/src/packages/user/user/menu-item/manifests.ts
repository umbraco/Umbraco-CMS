import { UMB_USER_MANAGEMENT_MENU_ALIAS } from '../../section/menu/constants.js';
import { UMB_USER_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Users',
		name: 'Users Menu Item',
		weight: 200,
		meta: {
			label: '#treeHeaders_users',
			icon: 'icon-user',
			entityType: UMB_USER_ROOT_ENTITY_TYPE,
			menus: [UMB_USER_MANAGEMENT_MENU_ALIAS],
		},
	},
];
