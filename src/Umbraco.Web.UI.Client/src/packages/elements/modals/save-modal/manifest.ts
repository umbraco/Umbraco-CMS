import { UMB_ELEMENT_SAVE_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifest: ManifestModal = {
	type: 'modal',
	alias: UMB_ELEMENT_SAVE_MODAL_ALIAS,
	name: 'Element Save Modal',
	element: () => import('./element-save-modal.element.js'),
};
