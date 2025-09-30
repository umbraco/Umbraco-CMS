import { UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL_ALIAS,
		name: 'Entity Create Option Action List Modal',
		element: () => import('./entity-create-option-action-list-modal.element.js'),
	},
];
