import { UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_SCHEDULE_MODAL_ALIAS,
		name: 'Document Schedule Modal',
		element: () => import('./document-schedule-modal.element.js'),
	},
];
