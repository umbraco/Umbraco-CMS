import { UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL_ALIAS,
		name: 'Sort Children Of Document Modal',
		element: () => import('./sort-children-of-document-modal.element.js'),
	},
];
