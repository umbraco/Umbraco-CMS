import { UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL } from './modal/constants.js';
import { UmbSortChildrenOfContentEntityAction } from '@umbraco-cms/backoffice/content';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from '@umbraco-cms/backoffice/tree';

/**
 * Entity action for sorting children of a document
 * @class UmbSortChildrenOfDocumentEntityAction
 * @augments UmbSortChildrenOfContentEntityAction
 */
export class UmbSortChildrenOfDocumentEntityAction extends UmbSortChildrenOfContentEntityAction {
	protected override _getModalToken(): UmbModalToken<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
		return UMB_SORT_CHILDREN_OF_DOCUMENT_MODAL;
	}
}

export { UmbSortChildrenOfDocumentEntityAction as api };
