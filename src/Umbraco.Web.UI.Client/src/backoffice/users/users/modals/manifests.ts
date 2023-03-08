import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.UserPicker',
		name: 'User Picker Modal',
		loader: () => import('./user-picker/user-picker-modal.element'),
	},
];

export const manifests = [...modals];
