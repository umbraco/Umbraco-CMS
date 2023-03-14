import type { ManifestModal } from '@umbraco-cms/extensions-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.LanguagePicker',
		name: 'Language Picker Modal',
		loader: () => import('./language-picker/language-picker-modal.element'),
	},
];

export const manifests = [...modals];
