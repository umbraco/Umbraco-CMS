import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_LANGUAGE_PICKER_MODAL_ALIAS = 'Umb.Modal.DocumentLanguagePicker';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_LANGUAGE_PICKER_MODAL_ALIAS,
		name: 'Document Language Picker Modal',
		js: () => import('./language-picker/language-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
