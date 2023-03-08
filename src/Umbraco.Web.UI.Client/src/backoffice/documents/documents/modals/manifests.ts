import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentPicker',
		name: 'Document Picker Modal',
		loader: () => import('./document-picker/document-picker-modal.element'),
	},
];

export const manifests = [...modals];
