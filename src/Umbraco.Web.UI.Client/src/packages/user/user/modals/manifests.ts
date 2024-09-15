import { UMB_CREATE_USER_MODAL_ALIAS } from './create/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CREATE_USER_MODAL_ALIAS,
		name: 'Create User Modal',
		js: () => import('./create/create-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.CreateSuccess',
		name: 'Create Success User Modal',
		js: () => import('./create/create-user-success-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Picker',
		name: 'User Picker Modal',
		js: () => import('./user-picker/user-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Mfa',
		name: 'User Mfa Modal',
		js: () => import('./user-mfa/user-mfa-modal.element.js'),
	},
];
