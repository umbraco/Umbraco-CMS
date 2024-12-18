import { UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS,
		name: 'Document Publish With Descendants Modal',
		element: () => import('./document-publish-with-descendants-modal.element.js'),
	},
];
