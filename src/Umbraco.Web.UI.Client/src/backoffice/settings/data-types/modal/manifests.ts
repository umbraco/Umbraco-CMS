import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypePicker',
		name: 'Data Type Picker Modal',
		loader: () => import('./data-type-picker/data-type-picker-modal.element'),
	},
];

export const manifests = [...modals];
