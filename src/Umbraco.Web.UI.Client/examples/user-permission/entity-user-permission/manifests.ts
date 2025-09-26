import {
	EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_1,
	EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_2,
} from '../constants.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '@umbraco-cms/backoffice/dictionary';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityUserPermission',
		alias: 'Example.EntityUserPermission.Entity.Action1',
		name: 'Action 1 for Entity User Permission',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		meta: {
			verbs: [EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_1],
			label: 'Action 1',
			description: 'Description for action 1',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Example.EntityUserPermission.Entity.Action2',
		name: 'Action 2 for Entity User Permission',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		meta: {
			verbs: [EXAMPLE_USER_PERMISSION_DICTIONARY_ACTION_2],
			label: 'Action 2',
			description: 'Description for action 2',
		},
	},
];
