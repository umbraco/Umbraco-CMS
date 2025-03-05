import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CharacterMap',
		name: 'Character Map Modal',
		element: () => import('./character-map-modal.element.js'),
	},
];
