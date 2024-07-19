import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { manifests as resendInviteManifests } from './resend-invite/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.ResendInvite',
		name: 'Resend Invite User Entity Action',
		weight: 500,
		api: () => import('./resend-invite/resend-invite.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-message',
			label: '#actions_resendInvite',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowResendInviteAction',
			},
		],
	},
	...resendInviteManifests,
];

export const manifests: Array<ManifestTypes> = [...entityActions];
