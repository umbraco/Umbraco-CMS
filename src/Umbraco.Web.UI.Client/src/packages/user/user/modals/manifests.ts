export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Picker',
		name: 'User Picker Modal',
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Mfa',
		name: 'User Mfa Modal',
		js: () => import('./user-mfa/user-mfa-modal.element.js'),
	},
];
