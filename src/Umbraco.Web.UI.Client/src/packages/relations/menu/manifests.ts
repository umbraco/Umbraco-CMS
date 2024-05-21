import { UMB_RELATION_TYPE_ROOT_ENTITY_TYPE } from '../relation-types/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Relations',
		name: 'Relations Menu Item',
		weight: 800,
		meta: {
			label: '#treeHeaders_relations',
			icon: 'icon-trafic',
			entityType: UMB_RELATION_TYPE_ROOT_ENTITY_TYPE,
			menus: ['Umb.Menu.AdvancedSettings'],
		},
	},
];
