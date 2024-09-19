import { UMB_MEMBER_MANAGEMENT_MENU_ALIAS } from '../../section/menu/constants.js';
import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Members',
		name: 'Members Menu Item',
		weight: 200,
		meta: {
			label: '#treeHeaders_member',
			icon: 'icon-user',
			entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
			menus: [UMB_MEMBER_MANAGEMENT_MENU_ALIAS],
		},
	},
];
