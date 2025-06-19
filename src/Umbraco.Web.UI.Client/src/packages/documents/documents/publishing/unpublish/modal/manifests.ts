import { UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS,
		name: 'Document Unpublish Modal',
		element: () => import('./document-unpublish-modal.element.js'),
	},
];
