export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUser',
		name: 'Current User Modal',
		element: () => import('./current-user/current-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserMfa',
		name: 'Current User MFA Modal',
		element: () => import('./current-user-mfa/current-user-mfa-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserMfaEnableProvider',
		name: 'Current User MFA Enable Provider Modal',
		element: () => import('./current-user-mfa-enable/current-user-mfa-enable-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserMfaDisableProvider',
		name: 'Current User MFA Disable Provider Modal',
		element: () => import('./current-user-mfa-disable/current-user-mfa-disable-modal.element.js'),
	},
];
