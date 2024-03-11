import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS = 'Umb.Modal.DocumentVariantPicker';
export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS,
		name: 'Document Variant Picker Modal',
		js: () => import('./variant-picker/document-variant-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
		name: 'Document Publish Modal',
		js: () => import('./variant-picker/document-variant-picker-modal.element.js'),
	},
];

export const manifests = [...modals];
