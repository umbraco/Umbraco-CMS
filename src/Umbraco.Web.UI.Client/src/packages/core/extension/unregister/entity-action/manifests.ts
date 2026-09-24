import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import { UmbUnregisterExtensionEntityAction } from './unregister-extension.action.js';
import { UMB_ACTION_GROUP_DELETE } from '@umbraco-cms/backoffice/action';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Extension.Unregister',
		group: UMB_ACTION_GROUP_DELETE,
		name: 'Unregister Extension Entity Action',
		api: UmbUnregisterExtensionEntityAction,
		forEntityTypes: [UMB_EXTENSION_ENTITY_TYPE],
		meta: {
			label: 'Unregister',
			icon: 'icon-trash',
		},
	},
];
