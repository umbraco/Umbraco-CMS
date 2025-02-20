export const UMB_CREATE_USER_MODAL_ALIAS = 'Umb.Modal.User.Create';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CREATE_USER_MODAL_ALIAS,
		name: 'Create User Modal',
		js: () => import('./create-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.CreateSuccess',
		name: 'Create Success User Modal',
		js: () => import('./create-user-success-modal.element.js'),
	},
];
