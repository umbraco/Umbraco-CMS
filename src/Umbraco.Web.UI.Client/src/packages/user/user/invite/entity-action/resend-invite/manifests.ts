import { UMB_USER_ENTITY_TYPE } from '../../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.ResendInvite',
		name: 'Resend Invite User Entity Action',
		weight: 500,
		api: () => import('./resend-invite.action.js'),
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
	{
		type: 'condition',
		name: 'User Allow Resend Invite Action Condition',
		alias: 'Umb.Condition.User.AllowResendInviteAction',
		api: () => import('./resend-invite.action.condition.js'),
	},
];
