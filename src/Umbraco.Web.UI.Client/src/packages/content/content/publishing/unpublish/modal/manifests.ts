import { UMB_CONTENT_UNPUBLISH_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CONTENT_UNPUBLISH_MODAL_ALIAS,
		name: 'Content Unpublish Modal',
		element: () => import('./content-unpublish-modal.element.js'),
	},
];
