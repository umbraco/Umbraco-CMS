import { UMB_USER_ROOT_ENTITY_TYPE } from '../../../entity.js';

import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
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
		},
	},
];
