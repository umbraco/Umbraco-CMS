import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.Current',
		name: 'Current User Modal',
		js: () => import('./current-user/current-user-modal.element.js'),
	},
];

export const manifests = [...modals];
