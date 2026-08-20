import { UMB_ENTITY_REFERENCES_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_ENTITY_REFERENCES_MODAL_ALIAS,
		name: 'Entity References Modal',
		element: () => import('./entity-references-modal.element.js'),
	},
];
