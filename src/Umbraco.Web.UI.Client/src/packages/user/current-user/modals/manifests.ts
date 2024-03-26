import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUser',
		name: 'Current User Modal',
		js: () => import('./current-user/current-user-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserMfa',
		name: 'Current User MFA Modal',
		js: () => import('./current-user-mfa/current-user-mfa-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserMfaEnableProvider',
		name: 'Current User MFA Enable Provider Modal',
		js: () => import('./current-user-mfa-enable-provider/current-user-mfa-enable-provider-modal.element.js'),
	},
];

export const manifests = [...modals];
