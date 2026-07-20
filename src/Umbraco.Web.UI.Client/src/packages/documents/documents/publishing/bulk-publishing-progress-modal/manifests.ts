import { UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL_ALIAS,
		name: 'Document Bulk Publishing Progress Modal',
		element: () => import('./bulk-publishing-progress-modal.element.js'),
	},
];
