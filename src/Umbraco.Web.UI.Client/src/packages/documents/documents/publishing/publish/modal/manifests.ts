import { UMB_DOCUMENT_PUBLISH_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
		name: 'Document Publish Modal',
		element: () => import('./document-publish-modal.element.js'),
	},
];
