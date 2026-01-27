import { UMB_ELEMENT_PUBLISH_MODAL_ALIAS } from './element-publish-modal.token.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ELEMENT_PUBLISH_MODAL_ALIAS,
		name: 'Element Publish Modal',
		element: () => import('./element-publish-modal.element.js'),
	},
];
