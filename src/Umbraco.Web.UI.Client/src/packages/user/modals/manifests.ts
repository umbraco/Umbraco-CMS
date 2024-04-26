import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ChangePassword',
		name: 'Change Password Modal',
		js: () => import('./change-password/change-password-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
