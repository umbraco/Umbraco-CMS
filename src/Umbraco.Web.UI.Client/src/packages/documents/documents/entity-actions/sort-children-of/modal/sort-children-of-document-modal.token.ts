import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbSortChildrenOfContentModalData } from '@umbraco-cms/backoffice/content';
import type { UmbSortChildrenOfModalValue } from '@umbraco-cms/backoffice/tree';

export const UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL_ALIAS = 'Umb.Modal.SortChildrenOfDocument';

export const UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL = new UmbModalToken<
	UmbSortChildrenOfContentModalData,
	UmbSortChildrenOfModalValue
>(UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
