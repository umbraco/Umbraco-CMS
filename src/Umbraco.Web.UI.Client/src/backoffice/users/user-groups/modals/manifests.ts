import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.UserGroupPicker',
		name: 'User Group Picker Modal',
		loader: () => import('./user-group-picker/user-group-picker-modal.element'),
	},
];

export const manifests = [...modals];
