import { UMB_CONTENT_ROLLBACK_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CONTENT_ROLLBACK_MODAL_ALIAS,
		name: 'Content Rollback Modal',
		element: () => import('./content-rollback-modal.element.js'),
	},
];
