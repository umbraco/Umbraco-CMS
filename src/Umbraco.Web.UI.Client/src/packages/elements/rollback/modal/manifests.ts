import { UMB_ELEMENT_ROLLBACK_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ELEMENT_ROLLBACK_MODAL_ALIAS,
		name: 'Element Rollback Modal',
		element: () => import('./rollback-modal.element.js'),
	},
];
