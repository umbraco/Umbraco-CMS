import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Create',
		name: 'Create User Modal',
		js: () => import('./create/user-create-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.CreateSuccess',
		name: 'Create Success User Modal',
		js: () => import('./create/user-create-success-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Invite',
		name: 'Invite User Modal',
		js: () => import('./invite/user-invite-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.ResendInvite',
		name: 'Resend Invite to User Modal',
		js: () => import('./resend-invite/resend-invite-to-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Picker',
		name: 'User Picker Modal',
		js: () => import('./user-picker/user-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
