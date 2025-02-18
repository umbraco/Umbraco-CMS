import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_SORT_CHILDREN_OF_MODAL_ALIAS = 'Umb.Modal.SortChildrenOf';

export interface UmbSortChildrenOfModalData extends UmbEntityModel {
	treeRepositoryAlias: string;
	sortChildrenOfRepositoryAlias: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
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
