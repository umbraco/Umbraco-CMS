import { UMB_MOVE_TO_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_MOVE_TO_MODAL_ALIAS,
		name: 'Move To Modal',
		element: () => import('./move-to-modal.element.js'),
	},
];
