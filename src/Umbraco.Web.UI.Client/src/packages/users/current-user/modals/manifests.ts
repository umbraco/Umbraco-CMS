import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUser',
		name: 'Current User Modal',
		loader: () => import('./current-user/current-user-modal.element'),
	},
];

export const manifests = [...modals];
