import { UMB_SORT_CHILDREN_OF_CONTENT_MODAL } from './constants.js';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import {
	UmbSortChildrenOfEntityAction,
	type UmbSortChildrenOfModalData,
	type UmbSortChildrenOfModalValue,
} from '@umbraco-cms/backoffice/tree';

/**
 * Entity action for sorting children of a content item
 * @class UmbSortChildrenOfContentEntityAction
 * @augments UmbSortChildrenOfEntityAction
 */
export class UmbSortChildrenOfContentEntityAction extends UmbSortChildrenOfEntityAction {
	protected override _getModalToken(): UmbModalToken<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
		return UMB_SORT_CHILDREN_OF_CONTENT_MODAL;
	}
}

export { UmbSortChildrenOfContentEntityAction as api };
