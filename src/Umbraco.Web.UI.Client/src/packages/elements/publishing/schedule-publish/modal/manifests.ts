import { UMB_ELEMENT_SCHEDULE_MODAL_ALIAS } from './element-schedule-modal.token.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ELEMENT_SCHEDULE_MODAL_ALIAS,
		name: 'Element Schedule Modal',
		element: () => import('./element-schedule-modal.element.js'),
	},
];
