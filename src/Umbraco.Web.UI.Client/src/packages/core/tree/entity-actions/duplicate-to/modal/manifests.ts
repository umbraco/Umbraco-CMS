import { UMB_DUPLICATE_TO_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DUPLICATE_TO_MODAL_ALIAS,
		name: 'Duplicate To Modal',
		element: () => import('./duplicate-to-modal.element.js'),
	},
];
