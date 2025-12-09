import { UMB_ENTITY_DELETE_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ENTITY_DELETE_MODAL_ALIAS,
		name: 'Entity Delete Modal',
		element: () => import('./entity-delete-modal.element.js'),
	},
];
