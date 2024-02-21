import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS = 'Umb.Modal.DocumentVariantPicker';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS,
		name: 'Document Variant Picker Modal',
		js: () => import('./variant-picker/document-variant-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
