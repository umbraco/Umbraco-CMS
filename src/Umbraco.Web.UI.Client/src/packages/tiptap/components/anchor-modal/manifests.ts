import { UMB_TIPTAP_ANCHOR_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_TIPTAP_ANCHOR_MODAL_ALIAS,
		name: 'Tiptap Anchor Modal',
		element: () => import('./anchor-modal.element.js'),
	},
];
