import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbItemDataResolverConstructor } from '@umbraco-cms/backoffice/entity-item';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from '@umbraco-cms/backoffice/tree';

export const UMB_SORT_CHILDREN_OF_CONTENT_MODAL_ALIAS = 'Umb.Modal.SortChildrenOfContent';

export interface UmbSortChildrenOfContentModalData extends UmbSortChildrenOfModalData {
	itemDataResolver?: UmbItemDataResolverConstructor;
}

export const UMB_SORT_CHILDREN_OF_CONTENT_MODAL = new UmbModalToken<
	UmbSortChildrenOfContentModalData,
	UmbSortChildrenOfModalValue
>(UMB_SORT_CHILDREN_OF_CONTENT_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
