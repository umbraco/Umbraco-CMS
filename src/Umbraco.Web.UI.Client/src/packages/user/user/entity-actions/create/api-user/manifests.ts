import { UMB_USER_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.User.Api',
		name: 'Api User Entity Create Option Action',
		weight: 1100,
		api: () => import('./api-user-entity-create-option-action.js'),
		forEntityTypes: [UMB_USER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-unplug',
			label: '#user_userKindApi',
			additionalOptions: true,
		},
	},
];
