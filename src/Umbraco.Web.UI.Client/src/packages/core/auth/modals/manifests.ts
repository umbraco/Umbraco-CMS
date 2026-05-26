import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import UmbAppAuthModalElement from './umb-app-auth-modal.element.js';
import UmbAuthTimeoutModalElement from './umb-auth-timeout-modal.element.js';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.AppAuth',
		name: 'Umb App Auth Modal',
		element: UmbAppAuthModalElement,
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.AuthTimeout',
		name: 'Umb Auth Timeout Modal',
		element: UmbAuthTimeoutModalElement,
	},
];
