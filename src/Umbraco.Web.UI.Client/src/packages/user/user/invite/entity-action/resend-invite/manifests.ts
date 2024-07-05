import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'User Allow Resend Invite Action Condition',
		alias: 'Umb.Condition.User.AllowResendInviteAction',
		api: () => import('./resend-invite.action.condition.js'),
	},
];
