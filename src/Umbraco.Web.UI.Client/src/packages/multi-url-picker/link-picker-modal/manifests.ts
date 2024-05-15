import { UMB_MULTI_URL_PICKER_MODAL_ALIAS } from './constants.js';

export const manifests = [
	{
		type: 'modal',
		alias: UMB_MULTI_URL_PICKER_MODAL_ALIAS,
		name: 'Property Editor Multi Url Link Picker Modal',
		element: () => import('./link-picker-modal.element.js'),
	},
];
