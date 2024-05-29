import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';

export const manifests = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Extension.Unregister',
		name: 'Unregister Extension Entity Action',
		api: () => import('./unregister-extension.action.js'),
		forEntityTypes: [UMB_EXTENSION_ENTITY_TYPE],
		meta: {
			label: 'Unregister',
			icon: 'icon-trash',
		},
	},
];
