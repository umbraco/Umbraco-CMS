import { UMB_DOCUMENT_PICKER_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PICKER_MODAL_ALIAS,
		name: 'Document Picker Modal',
		element: () => import('./document-picker-modal.element.js'),
	},
];
