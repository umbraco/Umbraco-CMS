import { UMB_SORT_CHILDREN_OF_CONTENT_MODAL } from './constants.js';
import type { MetaEntityActionSortChildrenOfContentKind } from './types.js';
import type { UmbSortChildrenOfContentModalData } from './modal/sort-children-of-content-modal.token.js';
import { UmbSortChildrenOfEntityAction } from '@umbraco-cms/backoffice/tree';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue } from '@umbraco-cms/backoffice/tree';

/**
 * Entity action for sorting children of a content item
 * @class UmbSortChildrenOfContentEntityAction
 * @augments UmbSortChildrenOfEntityAction
 */
export class UmbSortChildrenOfContentEntityAction extends UmbSortChildrenOfEntityAction {
	protected override _getModalToken(): UmbModalToken<UmbSortChildrenOfModalData, UmbSortChildrenOfModalValue> {
		return UMB_SORT_CHILDREN_OF_CONTENT_MODAL;
	}

	protected override _getModalData(): UmbSortChildrenOfContentModalData {
		const meta = this.args.meta as MetaEntityActionSortChildrenOfContentKind;
		return {
			...super._getModalData(),
			itemDataResolver: meta.itemDataResolver,
		};
	}
}

export { UmbSortChildrenOfContentEntityAction as api };
