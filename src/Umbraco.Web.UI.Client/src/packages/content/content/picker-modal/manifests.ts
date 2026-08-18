import { UMB_CONTENT_PICKER_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CONTENT_PICKER_MODAL_ALIAS,
		name: 'Content Picker Modal',
		element: () => import('./content-picker-modal.element.js'),
	},
];
