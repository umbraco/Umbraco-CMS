import { UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS,
		name: 'Character Map Modal',
		element: () => import('./character-map-modal.element.js'),
	},
];
