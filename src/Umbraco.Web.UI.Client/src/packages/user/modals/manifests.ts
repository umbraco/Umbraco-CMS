import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.ChangePassword',
		name: 'Change Password Modal',
		loader: () => import('./change-password/change-password-modal.element.js'),
	},
];

export const manifests = [...modals];
