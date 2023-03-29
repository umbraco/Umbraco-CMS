import type { ManifestModal } from '@umbraco-cms/backoffice/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentPicker',
		name: 'Document Picker Modal',
		loader: () => import('./document-picker/document-picker-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentTypePicker',
		name: 'Document Type Picker Modal',
		loader: () => import('./document-type-picker/document-type-picker-modal.element'),
	},
];

export const manifests = [...modals];
