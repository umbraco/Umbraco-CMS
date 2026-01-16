import { UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS } from './element-unpublish-modal.token.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS,
		name: 'Element Unpublish Modal',
		element: () => import('./element-unpublish-modal.element.js'),
	},
];
