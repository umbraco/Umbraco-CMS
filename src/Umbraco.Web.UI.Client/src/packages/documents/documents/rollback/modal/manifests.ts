import { UMB_ROLLBACK_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ROLLBACK_MODAL_ALIAS,
		name: 'Document Rollback Modal',
		element: () => import('./rollback-modal.element.js'),
	},
];
