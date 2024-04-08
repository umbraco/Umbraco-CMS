import { UMB_SORT_CHILDREN_OF_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbSortChildrenOfModalData {
	unique: string | null;
	entityType: string;
	treeRepositoryAlias: string;
	sortChildrenOfRepositoryAlias: string;
}

export interface UmbSortChildrenOfModalValue {}

export const UMB_SORT_CHILDREN_OF_MODAL = new UmbModalToken<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue>(
	UMB_SORT_CHILDREN_OF_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
