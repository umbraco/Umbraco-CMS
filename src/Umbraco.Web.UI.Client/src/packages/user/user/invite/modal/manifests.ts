import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Invite',
		name: 'Invite User Modal',
		js: () => import('./invite/user-invite-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Invite.Resend',
		name: 'Resend Invite to User Modal',
		js: () => import('./resend-invite/resend-invite-to-user-modal.element.js'),
	},
];

export const manifests = [...modals];
