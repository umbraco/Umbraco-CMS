import { UMB_SORT_CHILDREN_OF_MODAL_ALIAS } from './constants.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbSortChildrenOfModalData extends UmbEntityModel {
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
