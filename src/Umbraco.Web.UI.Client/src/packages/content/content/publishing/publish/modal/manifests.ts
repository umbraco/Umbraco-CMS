import { UMB_CONTENT_PUBLISH_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CONTENT_PUBLISH_MODAL_ALIAS,
		name: 'Content Publish Modal',
		element: () => import('./content-publish-modal.element.js'),
	},
];
