import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentPicker',
		name: 'Document Picker Modal',
	},
];

export const manifests = [...modals];
