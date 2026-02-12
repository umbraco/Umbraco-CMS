import {
	EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_1,
	EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_2,
} from '../constants.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '@umbraco-cms/backoffice/dictionary';
import { UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/user-permission';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Example.EntityAction.Action1',
		name: 'Action 1 Entity Action',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		api: () => import('./action-1.js'),
		weight: 10000,
		meta: {
			label: 'Action 1',
			icon: 'icon-car',
		},
		conditions: [
			{
				alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_1],
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Example.EntityAction.Action2',
		name: 'Action 2 Entity Action',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		api: () => import('./action-2.js'),
		weight: 9000,
		meta: {
			label: 'Action 2',
			icon: 'icon-bus',
		},
		conditions: [
			{
				alias: UMB_FALLBACK_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_2],
			},
		],
	},
];
