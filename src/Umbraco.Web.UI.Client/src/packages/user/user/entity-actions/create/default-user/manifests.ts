import { UMB_USER_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.User.Default',
		name: 'Default User Entity Create Option Action',
		weight: 1200,
		api: () => import('./default-user-entity-create-option-action.js'),
		forEntityTypes: [UMB_USER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-user',
			label: '#user_userKindDefault',
			additionalOptions: true,
		},
	},
];
