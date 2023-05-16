import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateUser',
		name: 'Create User Modal',
		loader: () => import('./create/user-create-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.InviteUser',
		name: 'Invite User Modal',
		loader: () => import('./invite/user-invite-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.UserPicker',
		name: 'User Picker Modal',
		loader: () => import('./user-picker/user-picker-modal.element'),
	},
];

export const manifests = [...modals];
