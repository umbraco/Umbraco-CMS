import { UMB_DUPLICATE_TO_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { UmbDuplicateToModalElement } from './duplicate-to-modal.element.js';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_DUPLICATE_TO_MODAL_ALIAS,
		name: 'Duplicate To Modal',
		element: UmbDuplicateToModalElement,
	},
];
