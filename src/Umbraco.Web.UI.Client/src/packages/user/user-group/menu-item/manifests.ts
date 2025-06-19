import { UMB_USER_MANAGEMENT_MENU_ALIAS } from '../../section/menu/constants.js';
import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.UserGroups',
		name: 'User Groups Menu Item',
		weight: 100,
		meta: {
			label: '#user_usergroups',
			icon: 'icon-users',
			entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE,
			menus: [UMB_USER_MANAGEMENT_MENU_ALIAS],
		},
	},
];
