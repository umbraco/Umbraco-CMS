export const manifests: Array<UmbExtensionManifest> = [
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
