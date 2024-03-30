import { UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../relation-types/index.js';

export const manifests = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Relations',
		name: 'Relations Menu Item',
		weight: 800,
		meta: {
			label: 'Relations',
			icon: 'icon-trafic',
			entityType: UMB_RELATION_TYPE_ROOT_ENTITY_TYPE,
			menus: ['Umb.Menu.AdvancedSettings'],
		},
	},
];
