import { UMB_SORT_CHILDREN_OF_MODAL_ALIAS } from './constants.js';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { UmbSortChildrenOfModalElement } from './sort-children-of-modal.element.js';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_SORT_CHILDREN_OF_MODAL_ALIAS,
		name: 'Sort Children Of Modal',
		element: UmbSortChildrenOfModalElement,
	},
];
