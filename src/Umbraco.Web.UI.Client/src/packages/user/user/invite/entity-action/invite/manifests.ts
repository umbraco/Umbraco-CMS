import { UMB_USER_ROOT_ENTITY_TYPE } from '../../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.Invite',
		name: 'Invite User Entity Action',
		weight: 1000,
		api: () => import('./invite-user-entity-action.js'),
		forEntityTypes: [UMB_USER_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-paper-plane',
			label: '#user_invite',
			additionalOptions: true,
		},
	},
];
