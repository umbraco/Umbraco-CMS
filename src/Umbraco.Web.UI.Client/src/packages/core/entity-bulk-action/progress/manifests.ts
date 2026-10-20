import { UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL_ALIAS,
		name: 'Entity Bulk Action Progress Modal',
		element: () => import('./entity-bulk-action-progress-modal.element.js'),
	},
];
