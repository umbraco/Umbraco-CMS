import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from '@umbraco-cms/backoffice/tree';

export const UMB_SORT_CHILDREN_OF_CONTENT_MODAL_ALIAS = 'Umb.Modal.SortChildrenOfContent';

export const UMB_SORT_CHILDREN_OF_CONTENT_MODAL = new UmbModalToken<
	UmbSortChildrenOfModalData,
	UmbSortChildrenOfModalValue
>(UMB_SORT_CHILDREN_OF_CONTENT_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
