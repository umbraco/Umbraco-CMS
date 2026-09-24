import { UMB_USER_ROOT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_ACTION_GROUP_CREATE } from '@umbraco-cms/backoffice/action';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.Invite',
		group: UMB_ACTION_GROUP_CREATE,
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
