import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Create',
		name: 'Create User Modal',
		loader: () => import('./create/user-create-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Invite',
		name: 'Invite User Modal',
		loader: () => import('./invite/user-invite-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.ResendInvite',
		name: 'Resend Invite to User Modal',
		loader: () => import('./resend-invite/resend-invite-to-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Picker',
		name: 'User Picker Modal',
		loader: () => import('./user-picker/user-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
