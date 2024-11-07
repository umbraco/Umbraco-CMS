export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ChangePassword',
		name: 'Change Password Modal',
		element: () => import('./change-password-modal.element.js'),
	},
];
